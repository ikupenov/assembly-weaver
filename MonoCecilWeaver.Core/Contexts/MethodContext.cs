using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoCecilWeaver.Target;
using ExceptionHandler = MonoCecilWeaver.Target.ExceptionHandler;

namespace MonoCecilWeaver.Core.Contexts
{
    public class MethodContext
    {
        public MethodContext(ModuleDefinition module, MethodDefinition method)
        {
            this.Module = module;
            this.Method = method;
        }

        public ModuleDefinition Module { get; }

        public MethodDefinition Method { get; }

        public MethodContext Catch<TException, THandler>()
            where TException : Exception
            where THandler : ExceptionHandler, new()
        {
            if (!Method.HasBody)
            {
                return this;
            }

            var exceptionHandlerType = typeof(THandler);

            var ilProcessor = Method.Body.GetILProcessor();
            var instructions = Method.Body.Instructions;

            Instruction loadReturnVariableInstruction = null;

            if (!Method.IsVoid())
            {
                var returnVariable = AddVariable(Method.ReturnType);
                loadReturnVariableInstruction = ilProcessor.Create(OpCodes.Ldloc, returnVariable);
            }

            var getCurrentMethodRef = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod));
            var getCurrentMethodInstruction = ilProcessor.Create(OpCodes.Call, Module.Import(getCurrentMethodRef));

            var loadInstanceInstruction = ilProcessor.Create(OpCodes.Ldnull);
            if (!Method.IsStatic)
            {
                loadInstanceInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
            }

            var returnInstruction = ilProcessor.Create(OpCodes.Ret);
            var leaveInstruction = Method.IsVoid()
                ? ilProcessor.Create(OpCodes.Leave, returnInstruction)
                : ilProcessor.Create(OpCodes.Leave, loadReturnVariableInstruction);

            var exceptionHandlerCtor = exceptionHandlerType.GetConstructor(Type.EmptyTypes);
            var createHandlerInstruction = ilProcessor.Create(OpCodes.Newobj, Module.Import(exceptionHandlerCtor));

            var handleExceptionInstruction = ilProcessor.Create(
                OpCodes.Callvirt,
                Module.Import(ExceptionHandler.MethodHandler<THandler>()));

            var exceptionVariable = AddVariable<Exception>();
            var storeExceptionInstruction = ilProcessor.Create(OpCodes.Stloc, exceptionVariable);
            var loadExceptionInstruction = ilProcessor.Create(OpCodes.Ldloc, exceptionVariable);

            {
                ilProcessor.Append(storeExceptionInstruction);

                ilProcessor.Append(createHandlerInstruction);

                ilProcessor.Append(loadExceptionInstruction);
                ilProcessor.Append(loadInstanceInstruction);
                ilProcessor.Append(getCurrentMethodInstruction);

                ilProcessor.Append(handleExceptionInstruction);

                ilProcessor.Append(leaveInstruction);

                if (!Method.IsVoid())
                {
                    ilProcessor.Append(loadReturnVariableInstruction);
                }

                ilProcessor.Append(returnInstruction);
            }

            AddExceptionHandler<TException>(
                ExceptionHandlerType.Catch,
                instructions.First(),
                storeExceptionInstruction,
                loadReturnVariableInstruction ?? returnInstruction);

            return this;
        }

        public MethodContext Rethrow<TException, THandler>()
            where TException : Exception
            where THandler : ExceptionHandler, new()
        {
            if (!Method.HasBody)
            {
                return this;
            }

            var exceptionHandlerType = typeof(THandler);

            var ilProcessor = Method.Body.GetILProcessor();
            var instructions = Method.Body.Instructions;

            var getCurrentMethodRef = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod));
            var getCurrentMethodInstruction = ilProcessor.Create(OpCodes.Call, Module.Import(getCurrentMethodRef));

            var loadInstanceInstruction = ilProcessor.Create(OpCodes.Ldnull);
            if (!Method.IsStatic)
            {
                loadInstanceInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
            }

            var returnInstruction = ilProcessor.Create(OpCodes.Ret);
            var leaveInstruction = ilProcessor.Create(OpCodes.Leave, returnInstruction);

            var exceptionHandlerCtor = exceptionHandlerType.GetConstructor(Type.EmptyTypes);
            var createHandlerInstruction = ilProcessor.Create(OpCodes.Newobj, Module.Import(exceptionHandlerCtor));

            var handleExceptionInstruction = ilProcessor.Create(
                OpCodes.Callvirt,
                Module.Import(ExceptionHandler.MethodHandler<THandler>()));

            var exceptionVariable = AddVariable<Exception>();
            var storeExceptionInstruction = ilProcessor.Create(OpCodes.Stloc, exceptionVariable);
            var loadExceptionInstruction = ilProcessor.Create(OpCodes.Ldloc, exceptionVariable);
            var rethrowExceptionInstruction = ilProcessor.Create(OpCodes.Rethrow);

            {
                ilProcessor.Append(storeExceptionInstruction);

                ilProcessor.Append(createHandlerInstruction);

                ilProcessor.Append(loadExceptionInstruction);
                ilProcessor.Append(loadInstanceInstruction);
                ilProcessor.Append(getCurrentMethodInstruction);

                ilProcessor.Append(handleExceptionInstruction);

                ilProcessor.Append(rethrowExceptionInstruction);

                ilProcessor.Append(leaveInstruction);
                ilProcessor.Append(returnInstruction);
            }

            AddExceptionHandler<TException>(
                ExceptionHandlerType.Catch,
                instructions.First(),
                storeExceptionInstruction,
                returnInstruction);

            return this;
        }

        public MethodContext Measure<TProfiler>()
            where TProfiler : PerformanceProfiler
        {
            if (!Method.HasBody)
            {
                return this;
            }

            var performanceProfilerType = typeof(TProfiler);

            var ilProcessor = Method.Body.GetILProcessor();
            var instructions = Method.Body.Instructions;
            var exceptionHandlers = Method.Body.ExceptionHandlers;

            Instruction loadReturnVariableInstruction = null;
            VariableDefinition returnVariable = null;

            if (!Method.IsVoid())
            {
                returnVariable = AddVariable(Method.ReturnType);
                loadReturnVariableInstruction = ilProcessor.Create(OpCodes.Ldloc, returnVariable);
            }

            var performanceProfilerVariable = AddVariable<TProfiler>();

            var performanceProfilerCtor = performanceProfilerType.GetConstructor(new[] { typeof(MethodBase) });
            var createProfilerInstruction = ilProcessor.Create(OpCodes.Newobj, Module.Import(performanceProfilerCtor));

            var storeProfilerInstruction = ilProcessor.Create(OpCodes.Stloc, performanceProfilerVariable);
            var loadProfilerInstruction = ilProcessor.Create(OpCodes.Ldloc, performanceProfilerVariable);
            var loadProfilerInstruction2 = ilProcessor.Create(OpCodes.Ldloc, performanceProfilerVariable);

            var startProfilerInstruction = ilProcessor.Create(
                OpCodes.Callvirt,
                Module.Import(PerformanceProfiler.StartHandler<TProfiler>()));

            var stopProfilerInstruction = ilProcessor.Create(
                OpCodes.Callvirt,
                Module.Import(PerformanceProfiler.StopHandler<TProfiler>()));

            var getCurrentMethodRef = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod));
            var getCurrentMethodInstruction = ilProcessor.Create(OpCodes.Call, Module.Import(getCurrentMethodRef));

            var endFinallyInstruction = ilProcessor.Create(OpCodes.Endfinally);
            var returnInstruction = ilProcessor.Create(OpCodes.Ret);

            ReplaceReturnWithLeaveInstructions(Method.Body, loadReturnVariableInstruction ?? returnInstruction, returnVariable);

            {
                ilProcessor.InsertAfter(instructions.First(), getCurrentMethodInstruction);
                ilProcessor.InsertAfter(getCurrentMethodInstruction, createProfilerInstruction);
                ilProcessor.InsertAfter(createProfilerInstruction, storeProfilerInstruction);
                ilProcessor.InsertAfter(storeProfilerInstruction, loadProfilerInstruction);
                ilProcessor.InsertAfter(loadProfilerInstruction, startProfilerInstruction);

                ilProcessor.Append(loadProfilerInstruction2);
                ilProcessor.Append(stopProfilerInstruction);
                ilProcessor.Append(endFinallyInstruction);

                if (!Method.IsVoid())
                {
                    ilProcessor.Append(loadReturnVariableInstruction);
                }

                ilProcessor.Append(returnInstruction);
            }

            AddExceptionHandler<Exception>(
                ExceptionHandlerType.Finally,
                instructions.First(),
                loadProfilerInstruction2,
                loadReturnVariableInstruction ?? returnInstruction);

            return this;
        }

        private VariableDefinition AddVariable(TypeReference typeReference)
        {
            var variable = new VariableDefinition(typeReference);
            Method.Body.Variables.Add(variable);

            return variable;
        }

        private VariableDefinition AddVariable(Type variableType)
        {
            return AddVariable(Module.Import(variableType));
        }

        private VariableDefinition AddVariable<TVariable>()
        {
            return AddVariable(Module.Import(typeof(TVariable)));
        }

        private Mono.Cecil.Cil.ExceptionHandler AddExceptionHandler<TException>(
            ExceptionHandlerType handlerType,
            Instruction tryStart,
            Instruction handlerStart,
            Instruction handlerEnd)
        {
            var exceptionHandler = new Mono.Cecil.Cil.ExceptionHandler(handlerType)
            {
                TryStart = tryStart,
                TryEnd = handlerStart,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
                CatchType = Module.Import(typeof(TException))
            };

            Method.Body.ExceptionHandlers.Add(exceptionHandler);

            return exceptionHandler;
        }

        private void ReplaceReturnWithLeaveInstructions(
            Mono.Cecil.Cil.MethodBody methodBody,
            Instruction leaveInstructionOperand,
            VariableDefinition returnVariable = null)
        {
            var ilProcessor = methodBody.GetILProcessor();
            var instructions = methodBody.Instructions;
            var exceptionHandlers = methodBody.ExceptionHandlers;

            Instruction storeReturnVariableInstruction = null;

            if (returnVariable != null)
            {
                storeReturnVariableInstruction = ilProcessor.Create(OpCodes.Stloc, returnVariable);
            }

            foreach (var instruction in instructions.ToList())
            {
                if (instruction.OpCode == OpCodes.Ret)
                {
                    var leaveInstruction = ilProcessor.Create(OpCodes.Leave, leaveInstructionOperand);
                    ilProcessor.Replace(instruction, leaveInstruction);

                    if (returnVariable != null)
                    {
                        ilProcessor.InsertBefore(leaveInstruction, storeReturnVariableInstruction);
                    }

                    instructions
                        .ToList()
                        .ForEach(i =>
                        {
                            if (i.Operand is Instruction operandInstruction &&
                                operandInstruction.Equals(instruction))
                            {
                                i.Operand = leaveInstruction;
                            }
                        });

                    exceptionHandlers
                        .ToList()
                        .ForEach(eHandler =>
                        {
                            if (eHandler.TryStart.Equals(instruction))
                            {
                                eHandler.TryStart = storeReturnVariableInstruction ?? leaveInstruction;
                            }

                            if (eHandler.TryEnd.Equals(instruction))
                            {
                                eHandler.TryEnd = storeReturnVariableInstruction ?? leaveInstruction;
                            }

                            if (eHandler.HandlerStart.Equals(instruction))
                            {
                                eHandler.HandlerStart = storeReturnVariableInstruction ?? leaveInstruction;
                            }

                            if (eHandler.HandlerEnd.Equals(instruction))
                            {
                                eHandler.HandlerEnd = storeReturnVariableInstruction ?? leaveInstruction;
                            }
                        });
                }
            }
        }
    }
}
