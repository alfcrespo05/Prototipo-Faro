using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    public List<Item> objetos = new List<Item>();

    // =========================================================
    // AÑADIR OBJETO
    // =========================================================

    public void AñadirObjeto(TipoObjeto tipo, int cantidad = 1)
    {
        // Comprobamos si ya tenemos ese objeto
        Item objetoExistente = objetos.Find(
            x => x.tipo == tipo
        );

        if (objetoExistente != null)
        {
            objetoExistente.cantidad += cantidad;
        }
        else
        {
            objetos.Add(
                new Item(tipo, cantidad)
            );
        }

        Debug.Log(
            "Inventario: +" +
            cantidad +
            " " +
            tipo
        );
    }

    // =========================================================
    // COMPROBAR SI TENEMOS UN OBJETO
    // =========================================================

    public bool TieneObjeto(TipoObjeto tipo)
    {
        Item objeto = objetos.Find(
            x => x.tipo == tipo
        );

        return objeto != null && objeto.cantidad > 0;
    }

    // =========================================================
    // COMPROBAR CANTIDAD
    // =========================================================

    public int CantidadObjeto(TipoObjeto tipo)
    {
        Item objeto = objetos.Find(
            x => x.tipo == tipo
        );

        if (objeto == null)
            return 0;

        return objeto.cantidad;
    }

    // =========================================================
    // QUITAR OBJETO
    // =========================================================

    public bool QuitarObjeto(
        TipoObjeto tipo,
        int cantidad = 1
    )
    {
        Item objeto = objetos.Find(
            x => x.tipo == tipo
        );

        if (objeto == null)
            return false;

        if (objeto.cantidad < cantidad)
            return false;

        objeto.cantidad -= cantidad;

        if (objeto.cantidad <= 0)
        {
            objetos.Remove(objeto);
        }

        Debug.Log(
            "Inventario: -" +
            cantidad +
            " " +
            tipo
        );

        return true;
    }
}

