﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.EntryInterface
{
    class Directory:Entry
    {

        public List<Entry> entries {get; }
        public string name { set; get; }
        public int node { get; }
        public DateTime time { get; }


        public Directory(int nodeIndex,string name)
        {
            entries = new List<Entry>();
            this.name = name;
            this.node = nodeIndex;
            this.time = MemoryInterface.getInstance().getInodeByIndex(nodeIndex).getTime();
            int nodeTableBlock = MemoryInterface.getInstance().getInodeByIndex(nodeIndex).getBlock(0);
            for (int i = 2; ; i++)
            {
                string _name = MemoryInterface.getInstance().getDataBlockByIndex(nodeTableBlock).findInode(i);
                if (_name == null)
                {
                    break;
                }

           
              
                inode _node = MemoryInterface.getInstance().getInodeByIndex(MemoryInterface.getInstance().getInodeIndexByName(nodeIndex, _name));
           

                if (_node.getType().Equals("文件"))
                {
                    File file = new File(_node, _name);
                    entries.Add(file);                   
                }
                else
                {
                    Directory dir = new Directory(_node.id,_name);
                    entries.Add(dir);
                }            
            }
        }

        public DateTime getTime()
        {
            return time;
        }

        public string getName()
        {
            return name;
        }

        public string getType()
        {
            return "文件夹";
        }

        public int getSize()
        {
            return entries.Count;
        }

        public object getContent()
        {
            return entries;
        }

        private string initDir(List<int> blocks, int parent, int current)
        {
            string name = "新建文件夹";
            name = MemoryInterface.getInstance().addNewInodeTableItem(MemoryInterface.getInstance().getInodeByIndex(parent).getBlock(0), name, current);
            MemoryInterface.getInstance().cleanBlock(blocks);
            MemoryInterface.getInstance().addNewInodeTableItem(blocks[0], "..", parent);
            MemoryInterface.getInstance().addNewInodeTableItem(blocks[0], ".", current);
            MemoryInterface.getInstance().getInodeByIndex(current).init(current, blocks, "文件夹", DateTime.Now);
            return name;
        }

        public string createEntry(string _name ,string type)
        {
            int parent = -1;

            if (_name != null)      //点击文件夹创建
            {
                parent = MemoryInterface.getInstance().getInodeIndexByName(node, _name);     //选中文件夹为父目录
            }
            else        //直接新建
            {
                parent = node;       //当前目录为父目录
            }

            List<int> nodeLoc = MemoryInterface.getInstance().getRequireInodes(1);        //找到未使用的inode节点
            List<int> blockLoc = MemoryInterface.getInstance().getRequireBlocks(1);       //找到空闲磁盘块


            if (nodeLoc == null || blockLoc == null)        //inodeMap或blockMap用尽  
            {
                return null;
            }
            string name=null;
            if (type.Equals("文件夹"))
            {
                name = initDir(blockLoc, parent, nodeLoc[0]);
                entries.Add(new Directory(nodeLoc[0], name));
            }
            else
            {
                name = "新建文件";
                name = MemoryInterface.getInstance().addNewInodeTableItem(MemoryInterface.getInstance().getInodeByIndex(parent).getBlock(0), name, nodeLoc[0]);
                inode temp = MemoryInterface.getInstance().getInodeByIndex(nodeLoc[0]);
                temp.init(nodeLoc[0], blockLoc, "文件", DateTime.Now);
                entries.Add(new File(temp, name));
            }
            
            MemoryInterface.getInstance().write();

            
            return name;
        }

        public void reNameEntry(string newName,int _index)
        {
            inode _node = MemoryInterface.getInstance().getInodeByIndex(node);
            MemoryInterface.getInstance().getDataBlockByIndex(_node.getBlock(0)).reNameInode(newName, _index);      //在父目录的inodetable中进行修改
            MemoryInterface.getInstance().write();
        }

        public void removeEntry(int selectedItem,string name, inode n)
        {
            inode _node = null;
            if (n == null)
            {
                _node = MemoryInterface.getInstance().getInodeByIndex(node);
                entries.RemoveAt(selectedItem);
            }
            else
            {
                _node = n;
            }

            int id = MemoryInterface.getInstance().getDataBlockByIndex(_node.getBlock(0)).removeInode(name);      //找到删除文件的inode
            _node = MemoryInterface.getInstance().getInodeByIndex(id);

            if (_node.getType().Equals("文件夹"))
            {
                for (int i = 0; ; i++)       //释放inodetable中信息
                {
                    string _name = MemoryInterface.getInstance().getDataBlockByIndex(_node.getBlock(0)).findInode(2);
                    
                    if (_name != null)
                    {
                        removeEntry(-1, _name, MemoryInterface.getInstance().getInodeByIndex(id));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            List<int> b = _node.getBlockPtr().ToList<int>();        //获得该节点占用的全部块
            MemoryInterface.getInstance().releaseBlock(b);        //释放块位图
            MemoryInterface.getInstance().releaseInode(id);     //释放节点位图
            MemoryInterface.getInstance().write();
            
        }

    

        

        public bool write(string content)
        {
            return true;
        }
    }
}