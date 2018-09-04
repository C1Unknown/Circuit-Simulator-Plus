﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitSimulatorPlus
{
    public interface IClickable
    {
        Hitbox Hitbox
        {
            get;
        }
        bool IsSelected
        {
            get; set;
        }

        void Add();
        void Remove();
    }
}
