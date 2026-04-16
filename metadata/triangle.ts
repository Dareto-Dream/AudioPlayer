// AssemblyScript source for the embedded triangle-wave visualizer.

export function render(time: f32): f32 {
  const cycle = time * 0.85;
  const phase = cycle - Mathf.floor(cycle);
  return 1.0 - Mathf.abs(phase - 0.5) * 4.0;
}
