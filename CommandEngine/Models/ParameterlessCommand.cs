﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommandEngine.Models
{
    public abstract class ParameterlessCommand : Command
    {
        public override void CommandHandle(Console console, Tokenizer tokenizer)
        {
            if (tokenizer.Token != Token.EOF)
            {
                throw new IncorrectCommandFormatException("Parameterless commands cannot have parameters");
            }
            CommandAction();
        }
    }
}
