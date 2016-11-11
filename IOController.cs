﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FileSystem.EntryInterface;

namespace FileSystem
{
    public class IOController
    {
        private Directory workDir;      //当前工作目录
        private File selectedItem;       //选中的文件或文件夹

        private ListView listView;
        private List<Directory> backDir = new List<Directory>();        //记录上一个访问的文件
        private List<Directory> frontDir = new List<Directory>();


        public IOController(ListView listView) 
        {
            this.listView = listView;
            workDir = new Directory(0,"root");
            listItems();     
        }


        public void back()
        {
            if ( frontDir.Count == 0)
            {
                return;
            }
            backDir.Insert(0, workDir);
            workDir = frontDir.ElementAt(0);
            frontDir.RemoveAt(0);
            
            listItems();
        }

        public void forward()
        {
            if (backDir.Count == 0)
            {
                return;
            }
            frontDir.Insert(0, workDir);
            workDir = backDir.ElementAt(0);
            backDir.RemoveAt(0);

            listItems();
        }

        private void listItems()          //列出文件夹下的文件
        {
            listView.Items.Clear();
            List<Entry> entries = (List<Entry>)workDir.getContent();
            for (int i = 0; i<entries.Count ; i++)
            {
                Entry temp = entries.ElementAt<Entry>(i);
                string name = temp.getName();
                string type = temp.getType();
                DateTime time = temp.getTime();
                int size = temp.getSize();
                setViewItem(name, type, size, time);                              
            }
        }
        

        private void setViewItem(string _name, string type,int size,DateTime _time)     //显示文件属性
        {
            ListViewItem item=new ListViewItem();
            item.Text=_name;
            item.SubItems.Add(type);
            if (type.Equals("文件夹"))
            {
                item.SubItems.Add("");
                item.SubItems.Add(_time.ToString());
                item.ImageIndex = 0;
            }
            else
            {
                item.SubItems.Add(size.ToString()+"B");
                item.SubItems.Add(_time.ToString());
                item.ImageIndex = 1;
            }
            listView.Items.Add(item);  
        }

        public Boolean createEntry(string type,string _name)        //创建新的文件
        {
            string name = workDir.createEntry(_name,type);
            if (name == null)
            {
                return false;
            }

            if (_name == null)
            {
                setViewItem(name,type,0,DateTime.Now);
            }
            
            return true;
        }

        public void reNameEntry(string newName,int _index)       //重命名文件
        {
            workDir.reNameEntry(newName, _index);
        }

        public void removeEntry(string name,int index)
        {
            listView.Items.RemoveAt(index);
            workDir.removeEntry(index, name, null);
        }

        public Boolean reDirectCatalog(int selected)     //切换文件目录或打开文件
        {
            Entry temp = ((List<Entry>)workDir.getContent()).ElementAt<Entry>(selected);
            if (temp.GetType().Name.Equals("File"))        //是文件直接打开
            {
                selectedItem = (File)temp;
                return true;
            }
            else
            {
                frontDir.Insert(0, workDir);
                workDir = (Directory)temp;
            }
          
            listItems();     //是文件夹进入目录
            return false;
        }

        public string readFile()
        {
            return (string)selectedItem.getContent();          
        }

        public Boolean saveFile(string content)
        {
            
            if(selectedItem.write(content) == false)
            {
                return false;
            }
            listItems();
            return true;
        }

    }
}
