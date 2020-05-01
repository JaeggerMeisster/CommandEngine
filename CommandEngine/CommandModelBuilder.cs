﻿using CommandEngine.Exceptions;
using CommandEngine.Models;
using System;
using System.Linq;
using System.Reflection;

namespace CommandEngine
{
    internal static class CommandModelBuilder
    {
        /// <summary>
        /// Build data object from the command parameters
        /// </summary>
        /// <param name="tokenizer"></param>
        internal static object Build(Tokenizer tokenizer, Type commandDataType, CommandModelContext modelContext)
        {
            var commandDataInstance = Activator.CreateInstance(commandDataType);

            SetDefaults(commandDataInstance, commandDataType);

            // Handle positional arguments
            var count = modelContext.positionalProperties.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var property = modelContext.positionalProperties[i];
                    switch (tokenizer.Token)
                    {
                        case Token.Literal:
                            HandlePositionalLiteral(tokenizer, property, commandDataInstance);
                            break;

                        case Token.String:
                            HandlePositionalString(tokenizer, property, commandDataInstance);
                            break;

                        case Token.Number:
                            HandlePositionalNumber(tokenizer, property, commandDataInstance);
                            break;

                        case Token.Flag:
                            throw new IncorrectCommandFormatException($"Flags not supported as positional argument");
                        case Token.Key:
                            throw new IncorrectCommandFormatException($"Keys not supported as positional argument");
                        case Token.EOF:
                            throw new IncorrectCommandFormatException($"Missing value for argument[{i}] of type {property.PropertyType.ToString()}");
                    }
                }

                tokenizer.NextToken();
            }

            // Handle named arguments and flags
            while (tokenizer.Token != Token.EOF)
            {
                switch (tokenizer.Token)
                {
                    case Token.Flag:
                        HandleNamedFlag(tokenizer, modelContext, commandDataInstance);
                        break;

                    case Token.Key:
                        HandleNamedKey(tokenizer, modelContext, commandDataInstance);
                        break;

                    case Token.String:
                    case Token.Literal:
                    case Token.Number:
                        throw new IncorrectCommandFormatException("A value type should be proceded by a key");
                }

                tokenizer.NextToken();
            }

            return commandDataInstance;
        }

        /// <summary>
        /// Sets default values of the instance
        /// </summary>
        private static void SetDefaults(object commandDataInstance, Type commandDataType)
        {
            foreach (var property in commandDataType.GetProperties())
            {
                // Set booleans to false
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(commandDataInstance, false, null);
                }
            }
        }

        private static void HandlePositionalLiteral(Tokenizer tokenizer, PropertyInfo property, object commandDataInstance)
        {
            SetEnumProperty(tokenizer, property, commandDataInstance);
        }

        private static void HandlePositionalString(Tokenizer tokenizer, PropertyInfo property, object commandDataInstance)
        {
            SetStringProperty(tokenizer, property, commandDataInstance);
        }

        private static void HandlePositionalNumber(Tokenizer tokenizer, PropertyInfo property, object commandDataInstance)
        {
            SetNumberProperty(tokenizer, property, commandDataInstance);
        }

        /// <summary>
        /// Handle a leading flag token
        /// </summary>
        private static void HandleNamedFlag(Tokenizer tokenizer, CommandModelContext modelContext, object commandDataInstance)
        {
            var property = modelContext.aliasedProperties[tokenizer.Value];

            property.SetValue(commandDataInstance, true, null);
        }

        /// <summary>
        /// Handle a leading key token
        /// </summary>
        private static void HandleNamedKey(Tokenizer tokenizer, CommandModelContext modelContext, object commandDataInstance)
        {
            // Get property data
            var property = modelContext.aliasedProperties[tokenizer.Value];
            var propertyType = property.PropertyType;

            // Skip to next token, expecting a value type
            tokenizer.NextToken();

            switch (tokenizer.Token)
            {
                case Token.Literal:
                    SetEnumProperty(tokenizer, property, commandDataInstance);
                    break;

                case Token.Flag:
                    throw new IncorrectCommandFormatException("Cannot provide a flag after a key");
                case Token.Key:
                    throw new IncorrectCommandFormatException("Cannot provide a key after a key");
                case Token.String:
                    SetStringProperty(tokenizer, property, commandDataInstance);
                    break;

                case Token.Number:
                    SetNumberProperty(tokenizer, property, commandDataInstance);
                    break;

                case Token.EOF:
                    throw new IncorrectCommandFormatException("Expected a value after a key");
            }
        }

        /// <summary>
        /// Sets the value by string of an enum property
        /// </summary>
        private static void SetEnumProperty(Tokenizer tokenizer, PropertyInfo property, object commandDataInstance)
        {
            var propertyType = property.PropertyType;

            if (!propertyType.IsEnum)
            {
                throw new IncorrectCommandFormatException($"Expected an enum type, instead got type {propertyType}");
            }

            var names = Enum.GetNames(propertyType);
            var literalValue = tokenizer.Value;

            if (!names.Contains(literalValue))
            {
                throw new IncorrectCommandFormatException($"Expected one of the following values: {string.Join(", ", names)}");
            }

            property.SetValue(commandDataInstance, Enum.Parse(propertyType, literalValue));
        }

        /// <summary>
        /// Sets the value of a string property
        /// </summary>
        private static void SetStringProperty(Tokenizer tokenizer, PropertyInfo property, object commandDataInstance)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(string))
            {
                property.SetValue(commandDataInstance, tokenizer.Value, null);
            }
            else
            {
                throw new IncorrectCommandFormatException($"Expected a number type, found {propertyType} instead");
            }
        }

        /// <summary>
        /// Sets the value of a number (double, float, long or int) property
        /// </summary>
        private static void SetNumberProperty(Tokenizer tokenizer, PropertyInfo property, object commandDataInstance)
        {
            var propertyType = property.PropertyType;
            if (propertyType == typeof(double))
            {
                property.SetValue(commandDataInstance, double.Parse(tokenizer.Value), null);
            }
            else if (propertyType == typeof(float))
            {
                property.SetValue(commandDataInstance, float.Parse(tokenizer.Value), null);
            }
            else if (propertyType == typeof(long))
            {
                property.SetValue(commandDataInstance, long.Parse(tokenizer.Value), null);
            }
            else if (propertyType == typeof(int))
            {
                property.SetValue(commandDataInstance, int.Parse(tokenizer.Value), null);
            }
            else
            {
                throw new IncorrectCommandFormatException($"Expected a number type, found {propertyType} instead");
            }
        }
    }
}