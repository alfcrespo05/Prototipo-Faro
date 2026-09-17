using UnityEngine;
[System.Serializable]
public class Item
{
    public TipoObjeto tipo;
    public int cantidad;

    public Item(TipoObjeto tipo, int cantidad = 1)
    {
        this.tipo = tipo;
        this.cantidad = cantidad;
    }
}
