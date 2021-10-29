using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembler a = new Assembler();
            //to run tests, call the "TranslateAssemblyFile" function like this:
            //string sourceFileLocation = the path to your source file
            //string destFileLocation = the path to your dest file
            //a.TranslateAssemblyFile(sourceFileLocation, destFileLocation);
            a.TranslateAssemblyFile(@"C:\Users\liron\Desktop\Assembly examples\Add.asm", @"C:\Users\liron\Desktop\Assembly examples\Add.hack");
            //You need to be able to run two translations one after the other
            a.TranslateAssemblyFile(@"C:\Users\liron\Desktop\Assembly examples\MaxL.asm", @"C:\Users\liron\Desktop\Assembly examples\MaxL.hack");

            a.TranslateAssemblyFile(@"C:\Users\liron\Desktop\Assembly examples\Max.asm", @"C:\Users\liron\Desktop\Assembly examples\Max.hack");
            a.TranslateAssemblyFile(@"C:\Users\liron\Desktop\Assembly examples\ScreenExample.asm", @"C:\Users\liron\Desktop\Assembly examples\ScreenExample.hack");
            a.TranslateAssemblyFile(@"C:\Users\liron\Desktop\Assembly examples\SquareMacro.asm", @"C:\Users\liron\Desktop\Assembly examples\ScreenExample.hack");
            a.TranslateAssemblyFile(@"C:\Users\liron\Desktop\Assembly examples\TestJumping.asm", @"C:\Users\liron\Desktop\Assembly examples\TestJumping.hack");
            
        }
    }
}
