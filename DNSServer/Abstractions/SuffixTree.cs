using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class SuffixTree
    {
        private TreeNode root;
        
        public SuffixTree()
        {
            root = new TreeNode(new byte[0][], 0, -1);
        }

        //возвращает длину строки, которая является максимальным общим суффиксом и добавляет её адрес в sufAddr
        public int ReadNameAndUpdate(string name, ref int sufAddr, int nameAddr)
        {
            //создаем массивы кодов, чтобы удобнее работать с деревом
            var nameCodes = DNSGenerator.GenerateZones(name);

            var newName = nameCodes.Select(x => Encoding.UTF8.GetString(x)).ToArray();

            //добавочная длина, прибавляемая к адресу. Получается из длины всех массивов
            var nameLength = nameCodes.Select(x => x.Length + 1).Sum();
            
            //вызываем поиск с конца оповторяя рекурсивно от рута, до хоста
            int length = root.RNAU(nameCodes, nameCodes.Length - 1, ref sufAddr, nameAddr, nameLength, 0);
            return length;
        }
    }

    class TreeNode
    {
        private byte[] zoneName;
        private List<TreeNode> subZones;
        private int addr;

        public TreeNode(byte[][] codes, int ind, int lastAddress)
        {
            if (codes != null && ind < codes.Length && ind >= 0)
            {
                var zone = new byte[codes[ind].Length];
                codes[ind].CopyTo(zone, 0);
                zoneName = zone;
                addr = lastAddress - zone.Length - 1;
            }

            subZones = new List<TreeNode>();

            //конструируем пока не закончится слово
            if (ind > 0)
                subZones.Add(new TreeNode(codes, ind - 1, addr));
        }

        public int RNAU(byte[][] codes, int ind, ref int sufAddr, int startAddr, int fullLength, int lastLength)
        {
            //если мы прошли нулевой индекс имени, то пора заканчивать
            if (ind < 0)
            {
                sufAddr = addr;
                return lastLength;
            }
            
            //теперь проверим, входит ли слово в подзону
            foreach (var z in subZones)
            {
                //если зоны равны, то перейдем на следующий уровень дерева (который уже есть)
                if (codes[ind].SequenceEqual(z.zoneName))
                    return z.RNAU(codes, ind - 1, ref sufAddr, startAddr, fullLength, lastLength + z.zoneName.Length + 1);
            }

            //если такой зоны нету в дереве, то создадим её
            //добавим поддерево рекурсивно
            subZones.Add(new TreeNode(codes, ind, startAddr + fullLength - lastLength));

            //адрес макс. общего суффикса равен текущему адресу
            sufAddr = addr + fullLength - lastLength - codes[ind].Length - 1;
            return lastLength;
        }
    }
}
