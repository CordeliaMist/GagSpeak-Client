using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GagSpeak.Events;
public enum InitializationSteps
{
    CharacterHandlerInitialized,
    RS_ManagerInitialized,
    HardcoreManagerInitialized,
    MovementManagerInitialized,
    ActionManagerInitialized,
}

public class InitializationManager
{
    public List<InitializationSteps> CompletedSteps { get; } = new List<InitializationSteps>();
    public TaskCompletionSource<bool> _rsManagerReadyForEvent = new TaskCompletionSource<bool>();
    public TaskCompletionSource<bool> _hardcoreManagerReadyForEvent = new TaskCompletionSource<bool>();
    public TaskCompletionSource<bool> _OrdersReadyForEvent = new TaskCompletionSource<bool>();
    public TaskCompletionSource<bool> _actionManagerReadyForEvent = new TaskCompletionSource<bool>();
    public event Action? CharacterHandlerInitialized;
    public event Action? RS_ManagerInitialized;
    public event Action? HardcoreManagerInitialized;
    public event Action? MovementManagerInitialized;

    public void CompleteStep(InitializationSteps step) {
        CompletedSteps.Add(step);

        // if action manager is done, fire that
        if(CompletedSteps.Contains(InitializationSteps.ActionManagerInitialized)) {
            return;
        }

        // if movement control is done, fire that
        if(CompletedSteps.Contains(InitializationSteps.MovementManagerInitialized)) {
            // await for the action manager to be ready
            _actionManagerReadyForEvent.Task.Wait();
            // then invoke it
            MovementManagerInitialized?.Invoke();
            return;
        }

        // if hardcore manager is done, fire that
        if(CompletedSteps.Contains(InitializationSteps.HardcoreManagerInitialized)) {
            // await for the movement control to be ready
            _OrdersReadyForEvent.Task.Wait();
            // then invoke it
            HardcoreManagerInitialized?.Invoke();
            return;
        }

        // if our restraint set manager finished, fire that
        if(step == InitializationSteps.RS_ManagerInitialized) {
            // await for the character handler to be ready
            _hardcoreManagerReadyForEvent.Task.Wait();
            // then invoke it
            RS_ManagerInitialized?.Invoke();
            return;
        }

        // if our character handler is done, fire that
        if(step == InitializationSteps.CharacterHandlerInitialized) {
            // await for the restraint set manager to be ready
            _rsManagerReadyForEvent.Task.Wait();
            // then invoke it
            CharacterHandlerInitialized?.Invoke();
            return;
        }
    }
}