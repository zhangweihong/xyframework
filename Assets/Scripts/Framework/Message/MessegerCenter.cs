using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 消息管理中心
/// 建议少用，因为方便到项目后期
/// 如果开发人员较多会导致代码混乱不堪
/// 为了不必要的装箱拆箱可以多写些变体
/// 这里就写了一些基本类型的
/// </summary>
/// <typeparam name="MessageCenter"></typeparam>
public class MessageCenter : Singleton<MessageCenter>
{
    public class Message
    {
        public uint type;
        public System.Object[] objs;
    }

    public delegate void Fun<T>(T t);

    private Dictionary<uint, List<Fun<Message>>> mMap = new Dictionary<uint, List<Fun<Message>>>();

    public void RegisiterMessage(uint type, Fun<Message> fun)
    {
        if (!mMap.ContainsKey(type))
            mMap.Add(type, new List<Fun<Message>>());
        mMap[type].Add(fun);
    }

    public void UnRegisiterMessage(uint type, Fun<Message> fun)
    {
        if (mMap.ContainsKey(type))
        {
            mMap[type].Remove(fun);
        }
    }

    private void Send(Message msg)
    {
        List<Fun<Message>> list = mMap[msg.type];
        for (int i = list.Count - 1; i >= 0; --i)
        {
            if (list[i] == null || list[i].Target == null)
            {
                list.RemoveAt(i);
                continue;
            }
            list[i](msg);
        }
    }

    public void SendMessage(uint type)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            Send(msg);
        }
    }

    public void SendMessage(uint type, uint data)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = new object[1] { data };
            Send(msg);
        }
    }
    public void SendMessage(uint type, int data)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = new object[1] { data };
            Send(msg);
        }
    }
    public void SendMessage(uint type, float data)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = new object[1] { data };
            Send(msg);
        }
    }

    public void SendMessage(uint type, bool rule)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = new object[1] { rule };
            Send(msg);
        }
    }

    public void SendMessage(uint type, string data)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = new object[1] { data };
            Send(msg);
        }
    }

    public void SendMessage(uint type, MemoryStream data, uint msgid)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = new object[2] { data, msgid };
            Send(msg);
        }
    }

    public void SendMessage(uint type, params object[] args)
    {
        if (mMap.ContainsKey(type))
        {
            Message msg = new Message();
            msg.type = type;
            msg.objs = args;
            Send(msg);
        }
    }

    public bool ContainKey(uint type)
    {
        if (mMap.ContainsKey(type))
        {
            return true;
        }
        return false;
    }
}