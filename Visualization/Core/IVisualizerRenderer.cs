using System.Drawing;

namespace Spectrallis;

internal interface IVisualizerRenderer
{
    void Draw(Graphics graphics, Rectangle bounds, VisualizerScene scene);
}
