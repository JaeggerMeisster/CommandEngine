﻿using System;

namespace CommandEngine.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ArgumentModelAttribute : Attribute
    {
        internal string HelpText { get; }

        public ArgumentModelAttribute(string helpText)
        {
            this.HelpText = helpText;
        }
    }
}