using System;
using UnityEngine;

public class GSPlayerDetailsContainer {
    public GSPlayerDetails[] AllPlayerDetails;
}

[Serializable]
public class GSPlayerDetails
{
    public float peerId;
    public string playerId;
    public RGBColor color;

    public Color MaterialColor { get { return new Color(color.red/100, color.green/100, color.blue/100); } }

    public void SetRGBMaterialColor0to1(float r, float g, float b)
    {
        color = new RGBColor((int)r, (int)g, (int)b);
    }

    [Serializable]
    public class RGBColor
    {
        public RGBColor()
        {

        }

        public RGBColor(int r, int g, int b)
        {
            this.red = r;
            this.green = g;
            this.blue = b;
        }

        public int red;
        public int green;
        public int blue;
    }
}