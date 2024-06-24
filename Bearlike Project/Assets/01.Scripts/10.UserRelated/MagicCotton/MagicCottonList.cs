using System;
using System.Collections.Generic;
using UnityEngine;

namespace User.MagicCotton
{
    public class MagicCottonList : MonoBehaviour
    {
        [SerializeField] private List<MagicCottonBase> _magicCottonList = new List<MagicCottonBase>();

        public void SetList(List<MagicCottonBase> list) => _magicCottonList = list;
    }
}

