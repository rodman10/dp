﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.UndoStructs
{
    abstract class Cmd:AbstractUndoableCmd<dataBlock>
    {
        protected inode node;

        public Cmd(int node_index)
        {
            inode node = MemoryInterface.getInstance().getInodeByIndex(node_index);
            this.node = (inode)node.Clone();
            this.list = new List<dataBlock>();
            for (int i = 0; i < 13; i++)
            {
                if (node.getBlock(i) == 0)
                {
                    break;
                }
                list.Add((dataBlock)MemoryInterface.getInstance().getDataBlockByIndex(node.getBlock(i)).Clone());
            }
        }

        public override void undo()
        {
            canRedo = true;
        }
     
        public override void redo()
        {
            canUndo = true;
        }
        
    }
}
