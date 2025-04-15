public class ShipSpawnEvent
{
    public float time;        // Cuándo aparece el barco
    public int lane;          // En qué carril aparece (0, 1, 2)
    public bool isPirate;     // Si es pirata o no
    public int sizeIndex;     // Tamaño: 0 = pequeño, 1 = mediano, 2 = grande
    public float speed;       // Velocidad específica

    public ShipSpawnEvent(float t, int l, bool pirate, int size, float spd)
    {
        time = t;
        lane = l;
        isPirate = pirate;
        sizeIndex = size;
        speed = spd;
    }
}





