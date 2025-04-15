public class ShipSpawnEvent
{
    public float time;        // Cu�ndo aparece el barco
    public int lane;          // En qu� carril aparece (0, 1, 2)
    public bool isPirate;     // Si es pirata o no
    public int sizeIndex;     // Tama�o: 0 = peque�o, 1 = mediano, 2 = grande
    public float speed;       // Velocidad espec�fica

    public ShipSpawnEvent(float t, int l, bool pirate, int size, float spd)
    {
        time = t;
        lane = l;
        isPirate = pirate;
        sizeIndex = size;
        speed = spd;
    }
}





