using System;
using System.Collections.Generic;
using System.Linq;

namespace PatternSaga
{
    interface ICommand
    {
        void Invoke();
    }

    class ConcreteCommand<T> : ICommand
    {
        List<T> commandParams;
        public ConcreteCommand(List<T> param)
        {
            commandParams = param;
        }
        void ICommand.Invoke()
        {
            Console.WriteLine($"Command Invocked {commandParams[0]}");
            if ((int)(object)commandParams[0] == 2) throw new Exception("smth happend");
        }
    }

    class ConcreteAntiCommand<T> : ICommand
    {
        List<T> commandParams;
        public ConcreteAntiCommand(List<T> param)
        {
            commandParams = param;
        }
        void ICommand.Invoke()
        {
            Console.WriteLine($"Anti Command Invocked {commandParams[0]}");
        }
    }

    class CommandQuery
    {
        Queue<ICommand> commandList = new Queue<ICommand>();
        Queue<ICommand> antiCommands = new Queue<ICommand>();
        Stack<ICommand> antiCommandsExecute = new Stack<ICommand>();

        public void AppendCommand(ICommand command, ICommand antiCommand)
        {
            commandList.Enqueue(command);
            antiCommands.Enqueue(antiCommand);
        }

        public void ExecuteCommands()
        {
            try
            {
                var commands = commandList.Zip(antiCommands, (c, ac) => new { Command = c, AntiCommand = ac }); // для одновременной итерации по двум очередям
                foreach (var command in commands)
                {
                    antiCommandsExecute.Push(command.AntiCommand); // добавляем антикоманду в стек исполнения
                    command.Command.Invoke();
                }
            }
            catch
            {
                foreach (var antiCommand in antiCommandsExecute)
                {
                    antiCommand.Invoke();
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var CommandQuery = new CommandQuery();
            CommandQuery.AppendCommand(new ConcreteCommand<int>(new List<int> { 1 }), new ConcreteAntiCommand<int>(new List<int> { 1 }));
            CommandQuery.AppendCommand(new ConcreteCommand<int>(new List<int> { 2 }), new ConcreteAntiCommand<int>(new List<int> { 2 }));
            CommandQuery.ExecuteCommands();

        }
    }
}

