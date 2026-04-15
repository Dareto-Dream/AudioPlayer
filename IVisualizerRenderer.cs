using System.Drawing;

namespace AudioPlayer;

internal interface IVisualizerRenderer
{
    void Draw(Graphics graphics, Rectangle bounds, VisualizerScene scene);
}
