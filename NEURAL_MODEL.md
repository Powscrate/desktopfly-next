# Neural model: current implementation

## Model equation

Each simulated unit is a current-driven Leaky Integrate-and-Fire neuron. For timestep `Δt`:

`V(t + Δt) = V(t) + (Δt / τ) · (V_rest − V(t) + I(t))`

If `V ≥ V_threshold`, a spike is recorded and `V` is set to `V_reset`; the unit then ignores input for its configured refractory interval. Integration is explicit Euler and rejects non-finite input, non-positive `Δt`, and steps larger than `τ`.

Defaults in `LifNeuron`: resting potential −65 mV, threshold −50 mV, reset −65 mV, membrane time constant 20 ms, refractory period 2 ms. The unit treats input as a current-like drive in mV-equivalent units, not a calibrated biological current. Circuit-specific units use τ=10 ms / 2 ms refractory for the sensory unit and τ=15 ms / 3 ms for the motor unit.

## Shipped synthetic circuit

A normalized threat input [0,1] is converted to drive by multiplying by 25. Spikes from the sensory unit inject an immediate excitatory current of 23 into the escape-command unit. The second unit's spikes cause a motor impulse away from the cursor. These arbitrary values are chosen for a small, legible, testable closed loop; they are not fit to electrophysiology or a published circuit.

## Limitations

The current runtime has only two cells and one immediate connection. It does not yet provide arbitrary sparse adjacency lists, transmitter types, delayed synapses, adaptation, conductance-based synapses, stochastic channels, population models, event/raster export, imported connectomes, or biological validation. It uses a small fixed substep within the body update. LIF omits action-potential shape, active dendrites, ion-channel dynamics and many cell-specific properties. The word “neural” here describes a genuine, stateful LIF computation with causal motor influence—not a whole-brain model.
