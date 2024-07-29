#nullable enable
using System;
using UnityEngine.InputSystem;


namespace Nanover.Frontend.InputControlSystem.Utilities
{
    /// <summary>
    /// Manages a pair of input actions associated with buttons, translating them into a set of four
    /// mutually exclusive input states: i) Neither button pressed, ii) only A pressed, iii) only B
    /// pressed, or iv) both A & B pressed. This class plays a crucial role in facilitating the
    /// execution of distinct, mutually exclusive actions depending on the specific combination of
    /// buttons pressed. It effectively decouples the combined states of button inputs, allowing for
    /// more nuanced control over, and response to, user input.
    ///
    /// The class internally maintains a state that reflects the current combination of button presses.
    /// It subscribes to input actions and updates this state based on the actions performed. The state
    /// changes trigger corresponding events, enabling the class to act as a mediator between button
    /// input states and the actions they are intended to trigger.
    ///
    /// The design of this class makes it particularly useful in contexts where input actions are not
    /// independent, and the interaction between multiple inputs needs to be managed in a coherent and
    /// controlled manner.
    /// </summary>
    /// <example>
    /// Example Usage:
    /// This example demonstrates how to use the <c>InputActionDecoupler</c> to handle different button
    /// press combinations. Assume 'actionA' & 'actionB' are <see cref="InputAction">InputActions</see>
    /// associated with two different buttons.
    /// 
    /// <code>
    /// InputAction actionA = new InputAction("ButtonA", binding: "<Keyboard>/W");
    /// InputAction actionB = new InputAction("ButtonB", binding: "<Keyboard>/Q");
    /// 
    /// InputActionDecoupler decoupler = new InputActionDecoupler(actionA, actionB);
    /// 
    /// decoupler.A.performed += () => Debug.Log("A - performed");
    /// decoupler.B.performed += () => Debug.Log("B - performed");
    /// decoupler.AB.performed += () => Debug.Log("AB - performed");
    /// decoupler.A.canceled += () => Debug.Log("A - canceled");
    /// decoupler.B.canceled += () => Debug.Log("B - canceled");
    /// decoupler.AB.canceled += () => Debug.Log("AB - canceled");
    /// 
    /// // Enable the input actions
    /// actionA.Enable();
    /// actionB.Enable();
    /// 
    /// // Permit the decoupler to subscribe to the input actions. In a real-world scenario, the following
    /// // subscription controls can be triggered based on specific game states or user interactions.
    /// decoupler.PermitAB();
    /// 
    /// // To stop handling button B presses use the associated `Restrict` method. This functionality is
    /// // of use to entities like <see cref="HybridInputHandlers"/> which might only be permitted to
    /// // access one of the two available buttons under certain situations.  
    /// decoupler.RestrictB();
    /// 
    /// </code>
    /// 
    /// This example sets up the <c>InputActionDecoupler</c> with two buttons, A and B. It then
    /// subscribes to the different combined state events (A, B, and AB) and enables the handling
    /// of these button presses. The example also shows how to restrict the handling of button B
    /// presses (i.e. ignore button B press events).
    /// </example>
    /// <remarks>
    /// It is worth mentioning that currently this behaviour cannot be easily replicated directly
    /// using the Unity input action system alone; at least not without writing custom processors
    /// which would be overkill for a task such as this.
    ///  
    /// The inclusion of the <c>None</c> state event subscription primarily serves to maintain uniformity & 
    /// completeness in the class design, ensuring that all button state combinations are accounted for. 
    /// However, in most practical scenarios, the focus is generally on the active button press states 
    /// (<c>A</c>, <c>B</c>, and <c>AB</c>). As such, the <c>None</c> state event might be less
    /// frequently used, but it is available should a specific use case require its implementation.
    /// </remarks>
    public class InputActionDecoupler
    {
        /// Tracks the current internal state of the `InputActionDecoupler` instance.
        /// Four states are possible:
        /// - `None`: neither button is currently depressed.
        /// - `A`   : Only the "A" button is currently depressed.
        /// - `B`   : Only the "B" button is currently depressed.
        /// - `AB`  : both buttons are currently depressed.
        /// </summary>
        private States state = States.None;

        // Encapsulates the `state` field, facilitating the execution of entry and exit actions during
        // state transitions.
        private States State
        {
            get => state;
            set
            {
                // Reflexive transitions are disregarded as they do not signify meaningful change.
                if (state == value) return;

                // Trigger the cancellation event of the current state, followed by the performance
                // event of the new state.
                inputActionProxies[(int)state].InvokeCanceled();
                inputActionProxies[(int)value].InvokePerformed();

                // Update the state.
                state = value;
            }
        }

        /// <summary>
        /// Array of input actions for the two target buttons.
        /// </summary>
        private readonly InputAction[] inputActions = new InputAction[2];

        /// <summary>
        /// Indicates whether the input actions within `inputActions` have been subscribed to. A `true`
        /// value denotes that the `InputAction` at the corresponding index within `inputActions` has
        /// its `performed` & `canceled` events subscribed to.
        /// </summary>
        /// <remarks>
        /// Prevents redundant event subscriptions or errant unsubscriptions for taking place.
        /// </remarks>
        private readonly bool[] subscribedToInputActions = new bool[2];

        /// <summary>
        /// Array of input action proxies corresponding to the four possible mutually exclusive
        /// button combination states. The arrangement aligns with the enum states.
        /// </summary>
        private readonly InputActionProxy[] inputActionProxies = new InputActionProxy[4];


        /// <summary>
        /// Input action proxy for the state where neither button is pressed.
        /// </summary>
        public InputActionProxy None => inputActionProxies[(int)States.None];

        /// <summary>
        /// Input action proxy for the state where only the "A" button is pressed.
        /// </summary>
        public InputActionProxy A => inputActionProxies[(int)States.A];

        /// <summary>
        /// Input action proxy for the state where only the "B" button is pressed.
        /// </summary>
        public InputActionProxy B => inputActionProxies[(int)States.B];

        /// <summary>
        /// Input action proxy for the state where both the "A" and "B" buttons are pressed.
        /// </summary>
        public InputActionProxy AB => inputActionProxies[(int)States.AB];

        /// <summary>
        /// Initialise a new instance of the <see cref="InputActionDecoupler"/> class with the specified
        /// input actions for buttons "A" & "B". Input action proxies are created for each possible button
        /// state combination, enabling the subsequent subscription to their associated events.
        /// </summary>
        /// <param name="actionA">The input action associated with button "A".</param>
        /// <param name="actionB">The input action associated with button "B".</param>
        public InputActionDecoupler(InputAction actionA, InputAction actionB)
        {
            inputActions[0] = actionA;
            inputActions[1] = actionB;
            for (int i = 0; i < 4; i++) inputActionProxies[i] = new InputActionProxy();
        }

        /// <summary>
        /// Process an `InputAction` stimulus event and update the state accordingly.
        /// </summary>
        /// <param name="context">Callback context yielded by an `InputAction` event.</param>
        private void Process(InputAction.CallbackContext context)
        {
            // Determine the stimulus source (either "A" or "B" button).
            var stimulus = context.action == inputActions[0] ? States.A : States.B;
            // Update the state based on whether the button was pressed or released.
            State = context.phase == InputActionPhase.Performed ? state | stimulus : state & (~stimulus);
        }

        /// <summary>
        /// Subscribe or unsubscribe to the `performed` and `canceled` events of a target `InputAction`
        /// entity.
        /// </summary>
        /// <param name="target">The target button state for subscription.</param>
        /// <param name="subscribe">If `true`, events are subscribed to; otherwise, they are unsubscribed from.</param>
        private void Subscribe(States target, bool subscribe = true)
        {
            // Convert the `States` target into an integer capable of indexing the `inputActions`
            // and `subscribedToInputActions` arrays.
            var index = (int)target - 1;

            // Avoid redundant (un)subscription to events.
            if (subscribedToInputActions[index] == subscribe) return;

            if (subscribe)
            {
                // WARNING a test for inputActions[index].triggered needs to be made here as this code
                // does not correctly handle situations where the button it is subscribing to is already
                // held down.

                // Subscribe the `Process` method to the input action's `performed` & `canceled` events.
                inputActions[index].performed += Process;
                inputActions[index].canceled += Process;
            }
            else
            {
                // Unsubscribe the `Process` method to the input action's `performed` & `canceled` events.
                inputActions[index].performed -= Process;
                inputActions[index].canceled -= Process;
                // Care must be taken here as it is possible for an action to be unsubscribed from while
                // it is active. In such cases the state may need to be updated to account for its loss.
                State = state & (~target);
            }

            // Update the subscription tracker.
            subscribedToInputActions[index] = subscribe;
        }

        /// <summary>
        /// Subscribes to the events associated with the "A" button, enabling the processing of input
        /// actions related to the "A" button.
        /// </summary>
        public void PermitA() => Subscribe(States.A);

        /// <summary>
        /// Subscribes to the events associated with the "B" button, enabling the processing of input
        /// actions related to the "B" button.
        /// </summary>
        public void PermitB() => Subscribe(States.B);

        /// <summary>
        /// Subscribes to the events associated with the "A" & "B" buttons, enabling the processing of
        /// input actions related to the "A" & "B" buttons.
        /// </summary>
        public void PermitAB()
        {
            PermitA();
            PermitB();
        }

        /// <summary>
        ///Subscribes to the events associated with either the "A" or "B" button depending of
        /// the value of the index specified, enabling the processing of input actions related to
        /// the specified button. This allows for buttons to be targeted using an index.
        /// </summary>
        /// <param name="i">Index identifying the button to be restricted. Values of `0` & `1`
        /// indicate buttons "A" & "B" respectively.</param>
        public void PermitIndex(int i)
        {
            if (i == 0)
                PermitA();
            else if (i == 1)
                PermitB();
            else
                throw new IndexOutOfRangeException($"Index value must be 0 or 1 only; got {i}");
        }

        /// <summary>
        /// Unsubscribes from the events associated with the "A" button, preventing the processing of
        /// input actions related to the "A" button.
        /// </summary>
        public void RestrictA() => Subscribe(States.A, false);

        /// <summary>
        /// Unsubscribes from the events associated with the "B" button, preventing the processing of
        /// input actions related to the "B" button.
        /// </summary>
        public void RestrictB() => Subscribe(States.B, false);

        /// <summary>
        /// Unsubscribes from the events associated with the "A" & "B" buttons, preventing the processing
        /// of input actions related to the "A" & "B" buttons.
        /// </summary>
        public void RestrictAB()
        {
            RestrictA();
            RestrictB();
        }

        /// <summary>
        /// Unsubscribes from the events associated with either the "A" or "B" button depending of
        /// the value of the index specified, preventing the processing of input actions related to
        /// the specified button. This allows for buttons to be targeted using an index.
        /// </summary>
        /// <param name="i">Index identifying the button to be restricted. Values of `0` & `1`
        /// indicate buttons "A" & "B" respectively.</param>
        public void RestrictIndex(int i)
        {
            if (i == 0)
                RestrictA();
            else if (i == 1)
                RestrictB();
            else
                throw new IndexOutOfRangeException($"Index value must be 0 or 1 only; got {i}");
        }

        /// <summary>
        /// Enumerates the possible states and stimuli associated with the button inputs in the
        /// `InputActionDecoupler`. This enum captures both the static states of the buttons (e.g.,
        /// pressed or not pressed) and the dynamic stimuli that result from button interactions.
        /// </summary>
        [Flags]
        private enum States : byte
        {
            /// <summary>
            /// Represents the state where neither button is pressed, indicating an idle or default state.
            /// </summary>
            None = 0b00,

            /// <summary>
            /// Represents the state or stimulus when only the "A" button is pressed, indicating an action
            /// or response specific to this button.
            /// </summary>
            A = 0b01,

            /// <summary>
            /// Represents the state or stimulus when only the "B" button is pressed, indicating an action
            /// or response specific to this button.
            /// </summary>
            B = 0b10,

            /// <summary>
            /// Represents the combined state or stimulus when both the "A" and "B" buttons are pressed,
            /// indicating a distinct action or response to this combination.
            /// </summary>
            AB = A | B
        }


        /// <summary>
        /// Serves as a proxy for `Unity.InputSystem.InputAction`, facilitating controlled firing of
        /// `performed` and `canceled` events in the `InputActionDecoupler` class.
        /// </summary>
        public class InputActionProxy
        {
            /// <summary>
            /// Occurs when the associated input action is performed.
            /// </summary>
            public event Action? performed;

            /// <summary>
            /// Occurs when the associated input action is canceled.
            /// </summary>
            public event Action? canceled;

            /// <summary>
            /// Invokes the `performed` event.
            /// </summary>
            internal void InvokePerformed() => performed?.Invoke();

            /// <summary>
            /// Invokes the `canceled` event.
            /// </summary>
            internal void InvokeCanceled() => canceled?.Invoke();
        }
    }


}