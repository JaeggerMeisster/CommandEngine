﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommandEngine.Models
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public virtual string HelpText => "No help text provided";
        // Action of the command
        public abstract void CommandAction();
        // Handle of the command
        public abstract void CommandHandle(Console console, Tokenizer tokenizer);
    }
}
