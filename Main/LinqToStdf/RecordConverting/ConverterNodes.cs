﻿// (c) Copyright Mark Miller.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqToStdf.RecordConverting
{
    class InitializeRecordNode : CodeNode
    {
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitInitializeRecord(this);
        }
    }
    class EnsureCompatNode : CodeNode
    {
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitEnsureCompat(this);
        }
    }
    class InitReaderNode : CodeNode
    {
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitInitReaderNode(this);
        }
    }
    class TryFinallyNode : CodeNode
    {
        public TryFinallyNode(CodeNode tryNode, CodeNode finallyNode)
        {
            TryNode = tryNode;
            FinallyNode = finallyNode;
        }
        public CodeNode TryNode { get; private set; }
        public CodeNode FinallyNode { get; private set; }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitTryFinallyNode(this);
        }
    }
    class DisposeReaderNode : CodeNode
    {
        static MethodInfo _DisposeMethod = typeof(IDisposable).GetMethod("Dispose", new Type[0]);
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitDisposeReader(this);
        }
    }
    class BlockNode : CodeNode
    {
        public BlockNode(params CodeNode[] nodes) : this((IEnumerable<CodeNode>)nodes) { }
        public BlockNode(IEnumerable<CodeNode> nodes)
        {
            Nodes = nodes.ToList();
        }
        public List<CodeNode> Nodes { get; private set; }

        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
    class FieldAssignmentBlockNode : CodeNode
    {
        public BlockNode Block { get; private set; }
        public FieldAssignmentBlockNode(BlockNode node)
        {
            Block = node;
        }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitFieldAssignmentBlock(this);
        }
    }
    class ReturnRecordNode : CodeNode
    {
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitReturnRecord(this);
        }
    }
    class SkipRawBytesNode : CodeNode
    {
        static MethodInfo _SkipMethod = typeof(BinaryReader).GetMethod("Skip", typeof(int));
        public int Bytes { get; private set; }
        public SkipRawBytesNode(int bytes)
        {
            Bytes = bytes;
        }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitSkipRawBytes(this);
        }

    }
    class SkipTypeNode<T> : CodeNode
    {
        public int? LengthIndex { get; private set; }
        public bool IsNibble { get; private set; }
        public SkipTypeNode()
        {
            if (typeof(T).IsArray)
            {
                throw new InvalidOperationException("SkipTypeNode on an array type must be constructed with a length index.");
            }
        }
        public SkipTypeNode(int lengthIndex, bool isNibble = false)
        {
            if (!typeof(T).IsArray)
            {
                throw new InvalidOperationException("SkipTypeNode on an non-array type can't be constructed with a length index.");
            }
            LengthIndex = lengthIndex;
            if (isNibble && typeof(T) != typeof(byte)) throw new InvalidOperationException("Nibble arrays can only be read into byte arrays.");
            IsNibble = isNibble;
        }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitSkipType(this);
        }
    }
    class ReadFixedStringNode : CodeNode
    {
        public int Length { get; private set; }
        public ReadFixedStringNode(int length)
        {
            Length = length;
        }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitReadFixedString(this);
        }
    }
    class ReadTypeNode<T> : CodeNode
    {
        public int? LengthIndex { get; private set; }
        public bool IsNibble { get; private set; }
        public ReadTypeNode()
        {
            if (typeof(T).IsArray)
            {
                throw new InvalidOperationException("ReadTypeNode on an array type must be constructed with a length index.");
            }
        }
        public ReadTypeNode(int lengthIndex, bool isNibble = false)
        {
            if (!typeof(T).IsArray)
            {
                throw new InvalidOperationException("ReadTypeNode on an non-array type can't be constructed with a length index.");
            }
            LengthIndex = lengthIndex;
            if (isNibble && typeof(T) != typeof(byte[])) throw new InvalidOperationException("Nibble arrays can only be read into byte arrays.");
            IsNibble = isNibble;
        }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitReadType(this);
        }
    }
    class FieldAssignmentNode<T> : CodeNode
    {
        public FieldAssignmentNode(int index, CodeNode readNode, BlockNode assignmentBlock)
        {
            FieldIndex = index;
            ReadNode = readNode;
            AssignmentBlock = assignmentBlock;
        }
        public int FieldIndex { get; private set; }
        public CodeNode ReadNode { get; private set; }
        public BlockNode AssignmentBlock { get; private set; }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitFieldAssignment(this);
        }
    }
    class SkipAssignmentIfFlagSetNode : CodeNode
    {
        public SkipAssignmentIfFlagSetNode(int flagFieldIndex, byte flagMask)
        {
            FlagFieldIndex = flagFieldIndex;
            FlagMask = flagMask;
        }
        public int FlagFieldIndex { get; private set; }
        public byte FlagMask { get; private set; }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitSkipAssignmentIfFlagSet(this);
        }
    }
    class SkipAssignmentIfMissingValueNode<T> : CodeNode
    {
        public SkipAssignmentIfMissingValueNode(T missingValue)
        {
            MissingValue = missingValue;
        }
        public T MissingValue { get; private set; }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitSkipAssignmentIfMissingValue(this);
        }
    }
    class AssignFieldToPropertyNode<T> : CodeNode
    {
        public AssignFieldToPropertyNode(PropertyInfo property)
        {
            Property = property;
        }
        public PropertyInfo Property { get; private set; }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitAssignFieldToProperty(this);
        }
    }
    class SkipArrayAssignmentIfLengthIsZeroNode : CodeNode
    {
        public int LengthIndex { get; private set; }
        public SkipArrayAssignmentIfLengthIsZeroNode(int lengthIndex)
        {
            LengthIndex = lengthIndex;
        }

        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitSkipArrayAssignmentIfLengthIsZero(this);
        }
    }
    class ThrowInvalidOperationNode : CodeNode
    {
        public string Message { get; private set; }
        public ThrowInvalidOperationNode(string message)
        {
            Message = message;
        }
        public override CodeNode Accept(CodeNodeVisitor visitor)
        {
            return visitor.VisitThrowInvalidOperation(this);
        }
    }
}
