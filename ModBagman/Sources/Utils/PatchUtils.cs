using System.Reflection;
using System.Reflection.Emit;

namespace ModBagman;

/// <summary>
/// Provides helper methods for transpiling IL lists.
/// </summary>
public static class PatchUtils
{
    /// <summary>
    /// Returns the effect of the stack behaviour on the stack.
    /// </summary>
    /// <param name="behaviour">StackBehaviour to analyze.</param>
    /// <returns>
    /// Stack size change for this behaviour. 
    /// Positive numbers mean this behaviour adds this many values to the stack, 
    /// negative means removal of this many values from the stack.
    /// </returns>
    /// <exception cref="ArgumentException">The given StackBehaviour vaue is not part of the enum.</exception>
    public static int GetStackEffect(StackBehaviour behaviour) => behaviour switch
    {
        StackBehaviour.Pop0 => 0,
        StackBehaviour.Pop1 => -1,
        StackBehaviour.Pop1_pop1 => -2,
        StackBehaviour.Popi => -1,
        StackBehaviour.Popi_pop1 => -2,
        StackBehaviour.Popi_popi => -2,
        StackBehaviour.Popi_popi_popi => -3,
        StackBehaviour.Popi_popi8 => -2,
        StackBehaviour.Popi_popr4 => -2,
        StackBehaviour.Popi_popr8 => -2,
        StackBehaviour.Popref => -1,
        StackBehaviour.Popref_pop1 => -2,
        StackBehaviour.Popref_popi => -2,
        StackBehaviour.Popref_popi_pop1 => -3,
        StackBehaviour.Popref_popi_popi => -3,
        StackBehaviour.Popref_popi_popi8 => -3,
        StackBehaviour.Popref_popi_popr4 => -3,
        StackBehaviour.Popref_popi_popr8 => -3,
        StackBehaviour.Popref_popi_popref => -3,
        StackBehaviour.Push0 => 0,
        StackBehaviour.Push1 => 1,
        StackBehaviour.Push1_push1 => 2,
        StackBehaviour.Pushi => 1,
        StackBehaviour.Pushi8 => 1,
        StackBehaviour.Pushr4 => 1,
        StackBehaviour.Pushr8 => 1,
        StackBehaviour.Pushref => 1,
        StackBehaviour.Varpop => -1,
        StackBehaviour.Varpush => 1,
        _ => throw new ArgumentException("The given StackBehaviour is not a valid value.")
    };

    public static List<CodeInstruction> InsertAfterMethod(this List<CodeInstruction> code, MethodInfo target, List<CodeInstruction> insertedCode, int methodIndex = 0, int startOffset = 0, bool editsReturnValue = false, bool copyExceptionBlocks = true)
    {
        if (methodIndex < 0)
            throw new ArgumentException("methodIndex must not be negative.");

        if (startOffset < 0 || startOffset >= code.Count)
            throw new ArgumentException("startOffset must be within the bounds of the code.");

        int counter = methodIndex + 1;

        int index = startOffset;

        // Search for the method
        while (true)
        {
            if (index >= code.Count)
                throw new InvalidOperationException("Could not find the target method call.");

            if (code[index].Calls(target))
                counter--;

            if (counter == 0)
                break;

            index += 1;
        }

        bool noReturnValue = target.ReturnType == typeof(void);

        int stackDelta = noReturnValue || editsReturnValue ? 0 : 1;

        int firstIndex = index;

        // Find method end

        while (stackDelta > 0 && firstIndex < code.Count - 1)
        {
            firstIndex += 1;
            stackDelta += GetStackEffect(code[firstIndex].opcode.StackBehaviourPush);
            stackDelta += GetStackEffect(code[firstIndex].opcode.StackBehaviourPop);

            if (stackDelta < 0)
                throw new InvalidOperationException("Instructions after the method have an invalid state.");
        }

        if (stackDelta != 0)
            throw new InvalidOperationException("Could not calculate insert position.");

        // For methods that come right before scopes, shift labels and stuff

        insertedCode[^1].WithLabels(code[firstIndex + 1].labels.ToArray()).WithBlocks(code[firstIndex + 1].blocks.ToArray());
        code[firstIndex + 1].labels.Clear();

        // Also copy the exception block information associated with the method itself
        if (copyExceptionBlocks)
            insertedCode = insertedCode.Select(x => new CodeInstruction(x).WithBlocks(code[index].blocks.ToArray())).ToList();

        code.InsertRange(firstIndex + 1, insertedCode);

        return code;
    }

    public static List<CodeInstruction> InsertBeforeMethod(this List<CodeInstruction> code, MethodInfo target, List<CodeInstruction> insertedCode, int methodIndex = 0, int startOffset = 0, bool copyExceptionBlocks = true)
    {
        if (methodIndex < 0)
            throw new ArgumentException("methodIndex must not be negative.");

        if (startOffset < 0 || startOffset >= code.Count)
            throw new ArgumentException("startOffset must be within the bounds of the code.");

        int counter = methodIndex + 1;

        int index = startOffset;

        // Search for method
        while (true)
        {
            if (index >= code.Count)
                throw new InvalidOperationException("Could not find the target method call.");

            if (code[index].Calls(target))
                counter--;

            if (counter == 0)
                break;

            index += 1;
        }

        int firstIndex = index;

        int stackDelta = -1 * target.GetParameters().Length;

        if ((target.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
        {
            stackDelta -= 1;
        }

        // Find method start

        while (stackDelta < 0 && firstIndex > 0)
        {
            firstIndex -= 1;
            stackDelta += GetStackEffect(code[firstIndex].opcode.StackBehaviourPush);
            stackDelta += GetStackEffect(code[firstIndex].opcode.StackBehaviourPop);

            if (stackDelta > 0)
            {
                throw new InvalidOperationException("Instructions preceding the method have an invalid state.");
            }
        }

        if (stackDelta != 0)
        {
            throw new InvalidOperationException("Could not calculate insert position.");
        }

        // For methods that come right after scopes, shift labels and stuff
        insertedCode[0].WithLabels(code[firstIndex].labels.ToArray());
        code[firstIndex].labels.Clear();

        // Also copy the exception block information associated with the method itself
        if (copyExceptionBlocks)
            insertedCode = insertedCode.Select(x => new CodeInstruction(x).WithBlocks(code[index].blocks.ToArray())).ToList();

        code.InsertRange(firstIndex, insertedCode);

        return code;
    }

    public static List<CodeInstruction> InsertAroundMethod(this List<CodeInstruction> code, MethodInfo target, List<CodeInstruction> before, List<CodeInstruction> after, int methodIndex = 0, int startOffset = 0, bool editsReturnValue = false)
    {
        InsertAfterMethod(code, target, after, methodIndex, startOffset, editsReturnValue);
        InsertBeforeMethod(code, target, before, methodIndex, startOffset);

        return code;
    }

    public static List<CodeInstruction> InsertAt(this List<CodeInstruction> code, int position, List<CodeInstruction> insertedCode)
    {
        code.InsertRange(position, insertedCode);
        return code;
    }

    public static List<CodeInstruction> RemoveAt(this List<CodeInstruction> code, int position, int count)
    {
        code.RemoveRange(position, count);
        return code;
    }

    public static List<CodeInstruction> ReplaceAt(this List<CodeInstruction> code, int position, int count, List<CodeInstruction> insertedCode)
    {
        code.RemoveRange(position, count);
        code.InsertRange(position, insertedCode);
        return code;
    }

    public static List<CodeInstruction> ReplaceMethod(this List<CodeInstruction> code, MethodInfo target, MethodInfo replacement, int methodIndex = 0, int startOffset = 0)
    {
        var targetParams = target.GetParameters();
        var replacementParams = replacement.GetParameters();

        if (targetParams.Length != replacementParams.Length)
        {
            throw new InvalidOperationException("The target and the replacement have incompatible parameter lists");
        }

        for (int index = 0; index < targetParams.Length; index++)
        {
            var targetParam = targetParams[index];
            var replacementParam = replacementParams[index];

            bool notCompatible = targetParam.ParameterType != replacementParam.ParameterType ||
                targetParam.IsIn != replacementParam.IsIn ||
                targetParam.IsOut != replacementParam.IsOut ||
                targetParam.IsRetval != replacementParam.IsRetval;

            if (notCompatible)
            {
                throw new InvalidOperationException("The target and the replacement have incompatible parameter lists");
            }
        }

        for (int index = 0; index < code.Count; index++)
        {
            if (code[index].Calls(target))
            {
                code[index].operand = replacement;
            }
        }

        return code;
    }

    /// <summary>
    /// Returns the Nth position in the code list where the find criteria provided returns true.
    /// If not found, returns -1.
    /// </summary>
    public static int FindPosition(this List<CodeInstruction> code, Func<List<CodeInstruction>, int, bool> findCriteria, int skip = 0)
    {
        if (skip < 0)
            throw new InvalidOperationException("Skip must be higher than or equal to 0.");

        int leftToSkip = skip;

        for (int index = 0; index < code.Count; index++)
        {
            if (findCriteria(code, index))
            {
                if (leftToSkip == 0)
                    return index;

                leftToSkip--;
            }
        }

        return -1;
    }
}
