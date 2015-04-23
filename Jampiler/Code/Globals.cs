﻿using System.Collections.Generic;

namespace Jampiler.Code
{
    public class Globals
    {
        private static Globals _instance;

        public List<Global> List { get; set; }

        private Globals()
        {
            List = new List<Global>();
        }

        public static Globals Instance => _instance ?? (_instance = new Globals());
    }
}