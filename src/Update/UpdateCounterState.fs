module App.UpdateCounterState

open Elmish
open App.Types

let updateCounterState msg state =
    match msg with
    | IncrementCounter ->
        { state with
            Counter = state.Counter + 1 },
        Cmd.none
    | DecrementCounter ->
        { state with
            Counter = state.Counter - 1 },
        Cmd.none
