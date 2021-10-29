using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public class Assembler
    {
        private const int WORD_SIZE = 16;

        private Dictionary<string, int[]> m_dControl, m_dJmp; //these dictionaries map command mnemonics to machine code - they are initialized at the bottom of the class

        //more data structures here (symbol map, ...)
        private Dictionary <string,int>SymbolTable;

        public Assembler()
        {
            InitCommandDictionaries();
        }

        //this method is called from the outside to run the assembler translation
        public void TranslateAssemblyFile(string sInputAssemblyFile, string sOutputMachineCodeFile)
        {
            //read the raw input, including comments, errors, ...
            StreamReader sr = new StreamReader(sInputAssemblyFile);
            List<string> lLines = new List<string>();
            while (!sr.EndOfStream)
            {
                lLines.Add(sr.ReadLine());
            }
            sr.Close();
            //translate to machine code
            List<string> lTranslated = TranslateAssemblyFile(lLines);
            //write the output to the machine code file
            StreamWriter sw = new StreamWriter(sOutputMachineCodeFile);
            foreach (string sLine in lTranslated)
                sw.WriteLine(sLine);
            sw.Close();
        }

        //translate assembly into machine code
        private List<string> TranslateAssemblyFile(List<string> lLines)
        {
            //implementation order:
            //first, implement "TranslateAssemblyToMachineCode", and check if the examples "Add", "MaxL" are translated correctly.
            //next, implement "CreateSymbolTable", and modify the method "TranslateAssemblyToMachineCode" so it will support symbols (translating symbols to numbers). check this on the examples that don't contain macros
            //the last thing you need to do, is to implement "ExpendMacro", and test it on the example: "SquareMacro.asm".
            //init data structures here 

            //expand the macros
            List<string> lAfterMacroExpansion = ExpendMacros(lLines);

            //first pass - create symbol table and remove lable lines
            CreateSymbolTable(lAfterMacroExpansion);
            updateTable();

            //second pass - replace symbols with numbers, and translate to machine code
            List<string> lAfterTranslation = TranslateAssemblyToMachineCode(lAfterMacroExpansion);
            return lAfterTranslation;
        }

        
        //first pass - replace all macros with real assembly
        private List<string> ExpendMacros(List<string> lLines)
        {
            //You do not need to change this function, you only need to implement the "ExapndMacro" method (that gets a single line == string)
            List<string> lAfterExpansion = new List<string>();
            for (int i = 0; i < lLines.Count; i++)
            {
                //remove all redudant characters
                string sLine = CleanWhiteSpacesAndComments(lLines[i]);
                if (sLine == "")
                    continue;
                //if the line contains a macro, expand it, otherwise the line remains the same
                List<string> lExpanded = ExapndMacro(sLine);
                //we may get multiple lines from a macro expansion
                foreach (string sExpanded in lExpanded)
                {
                    lAfterExpansion.Add(sExpanded);
                }
            }
            return lAfterExpansion;
        }

        //expand a single macro line
        private List<string> ExapndMacro(string sLine)
        {
            List<string> lExpanded = new List<string>();
            string temp;
            if (IsCCommand(sLine))
            {
                int x=0;
                string sDest, sCompute, sJmp;
                GetCommandParts(sLine, out sDest, out sCompute, out sJmp);
                //your code here - check for indirect addessing and for jmp shortcuts
                //read the word file to see all the macros you need to support
                if(sCompute.Contains("++"))
                {
                    int i=sCompute.IndexOf('+'); //macro: i++
                    temp="@"+sCompute.Substring(0,i);
                    lExpanded.Add(temp);
                    temp="M=M+1";
                    lExpanded.Add(temp);
                }
                else if(sCompute.Contains("--"))
                {
                    int i=sCompute.IndexOf('-'); //macro: i++
                    temp="@"+sCompute.Substring(0,i);
                    lExpanded.Add(temp);
                    temp="M=M-1";
                    lExpanded.Add(temp);
                }
                else if(int.TryParse(sCompute,out x)&&sJmp==""&&(!sDest.Contains("A") && !sDest.Contains("M") &&!sDest.Contains("D"))) //macro: x=5
                {
                    temp="@"+sCompute;
                    lExpanded.Add(temp);
                    temp="D=A";
                    lExpanded.Add(temp);
                    temp="@"+sDest;
                    lExpanded.Add(temp);
                    temp="M=D";
                    lExpanded.Add(temp);
                }
                else if(sJmp.Contains(":")) //macro: D;JGT:LOOP
                {
                    int i=sJmp.IndexOf(':');
                    temp="@"+sJmp.Substring(i+1);
                    lExpanded.Add(temp);
                    temp="D;"+sJmp.Substring(0,i);
                    lExpanded.Add(temp);
                }
                else if (sJmp==""&&(!sDest.Contains("A") && !sDest.Contains("M") &&!sDest.Contains("D")) && (!sCompute.Contains("A") && !sCompute.Contains("M") &&!sCompute.Contains("D")))
                {
                    temp="@"+sCompute;
                    lExpanded.Add(temp);
                    temp="D=M";
                    lExpanded.Add(temp);
                    temp="@"+sDest;
                    lExpanded.Add(temp);
                    temp="M=D";
                    lExpanded.Add(temp);
                }
                else if(sJmp==""&&!sCompute.Contains("A") && !sCompute.Contains("M") &&!sCompute.Contains("D") &&!sCompute.Contains("0") &&!sCompute.Contains("1"))
                {
                    temp="@"+sCompute;
                    lExpanded.Add(temp);
                    temp=sDest+"=M";
                    lExpanded.Add(temp);
                }
                else if(sDest!=""&&!sDest.Contains("A") && !sDest.Contains("M") &&!sDest.Contains("D"))
                {
                    temp="@"+sDest;
                    lExpanded.Add(temp);
                    temp="M="+sCompute;
                    lExpanded.Add(temp);
                }
            }
            if (lExpanded.Count == 0)
                lExpanded.Add(sLine);
            return lExpanded;
        }

        //second pass - record all symbols - labels and variables
        private void CreateSymbolTable(List<string> lLines)
        {
            string sLine = "";
            string label;
            int j=0;
            SymbolTable=new Dictionary<string, int>();
            for (int i=0;i<16;i++)
            {
                label="R"+Convert.ToString(i);
                SymbolTable.Add(label,i);
            }
            SymbolTable.Add("SCREEN",16384);
            SymbolTable.Add("KBD",24576);
            for (int i = 0; i < lLines.Count; i++)
            {
                sLine = lLines[i];
                if (IsLabelLine(sLine))
                {
                    //record label in symbol table
                    //do not add the label line to the result
                    label=sLine.Substring(1,sLine.Length-2);
                    if(SymbolTable.ContainsKey(label)&&SymbolTable[label]!=-1)
                        throw new FormatException("Cannot use this label name again " + i + ": " + lLines[i]);
                    if (SymbolTable.ContainsKey(label))
                        SymbolTable[label]=j;
                    else
                        SymbolTable.Add(label,j);
                    lLines.Remove(lLines[i]);
                    j--;
                    i--;
                }
                else if (IsACommand(sLine))
                {
                    if ((sLine[1]<=57&&sLine[1]>=48)&&sLine.Any(char.IsLetter))
                        throw new FormatException("The label " + lLines[i] + " does not legal" );
                    //may contain a variable - if so, record it to the symbol table (if it doesn't exist there yet...)
                    if(sLine[1]>57||sLine[1]<48)
                    {
                        label=sLine.Substring(1,sLine.Length-1);
                        if(!SymbolTable.ContainsKey(label))
                        {
                            SymbolTable.Add(label,-1);
                        }
                    }
                }
                else if (IsCCommand(sLine))
                {
                    //do nothing here
                }
                else
                    throw new FormatException("Cannot parse line " + i + ": " + lLines[i]);
                j++;
            }
        }
        
        //third pass - translate lines into machine code, replacing symbols with numbers
        private List<string> TranslateAssemblyToMachineCode(List<string> lLines)
        {
            string sLine = "";
            List<string> lAfterPass = new List<string>();
            for (int i = 0; i < lLines.Count; i++)
            {
                sLine = lLines[i];
                string strNum;
                if (IsACommand(sLine))
                {
                    //translate an A command into a sequence of bits
                    if(sLine[1]<=57&&sLine[1]>=48)
                    {
                        strNum=sLine.Substring(1);
                    }
                    else
                    {
                        strNum=Convert.ToString(SymbolTable[sLine.Substring(1)]);
                    }
                    int num;
                    num=Convert.ToInt32(strNum);
                    if (num>=32768)
                        throw new FormatException("The value " +strNum +" is not legal in line "+i);
                    lAfterPass.Add(ToBinary(num));
                }
                else if (IsCCommand(sLine))
                {
                    string sDest, sControl, sJmp;
                    GetCommandParts(sLine, out sDest, out sControl, out sJmp);
                    //translate an C command into a sequence of bits
                    //take a look at the dictionaries m_dControl, m_dJmp, and where they are initialized (InitCommandDictionaries), to understand how to you them here
                    string temp="100";
                    if(!m_dControl.ContainsKey(sControl))
                        throw new FormatException("The value of the controls "+sControl +" is not legal in line " + i);
                    temp+=ToString(m_dControl[sControl]);
                    if(sJmp==""&&sDest.Contains("A"))
                        temp+="1";
                    else
                    temp+="0";
                    if(sJmp==""&&sDest.Contains("D"))
                        temp+="1";
                    else
                    temp+="0";
                    if(sJmp==""&&sDest.Contains("M"))
                        temp+="1";
                    else
                    temp+="0";
                    if(!m_dJmp.ContainsKey(sJmp))
                        throw new FormatException("The value of the jump is not legal in line " + i);
                    temp+=ToString(m_dJmp[sJmp]);
                    lAfterPass.Add(temp);
                }
                else
                    throw new FormatException("Cannot parse line " + i + ": " + lLines[i]);
            }
            return lAfterPass;
        }

        private void updateTable ()
        {
            int j=16;
            List<string> keys = new List<string>(SymbolTable.Keys);
                foreach (string key in keys)
                {
                    if (SymbolTable[key]==-1)
                    {
                        SymbolTable[key]=j;
                        j++;
                    }
                }
        }

        //helper functions for translating numbers or bits into strings
        private string ToString(int[] aBits)
        {
            string sBinary = "";
            for (int i = 0; i < aBits.Length; i++)
                sBinary += aBits[i];
            return sBinary;
        }

        private string ToBinary(int x)
        {
            string sBinary = "";
            for (int i = 0; i < WORD_SIZE; i++)
            {
                sBinary = (x % 2) + sBinary;
                x = x / 2;
            }
            return sBinary;
        }


        //helper function for splitting the various fields of a C command
        private void GetCommandParts(string sLine, out string sDest, out string sControl, out string sJmp)
        {
            if (sLine.Contains('='))
            {
                int idx = sLine.IndexOf('=');
                sDest = sLine.Substring(0, idx);
                sLine = sLine.Substring(idx + 1);
            }
            else
                sDest = "";
            if (sLine.Contains(';'))
            {
                int idx = sLine.IndexOf(';');
                sControl = sLine.Substring(0, idx);
                sJmp = sLine.Substring(idx + 1);

            }
            else
            {
                sControl = sLine;
                sJmp = "";
            }
        }

        private bool IsCCommand(string sLine)
        {
            return !IsLabelLine(sLine) && sLine[0] != '@';
        }

        private bool IsACommand(string sLine)
        {
            return sLine[0] == '@';
        }

        private bool IsLabelLine(string sLine)
        {
            if (sLine.StartsWith("(") && sLine.EndsWith(")"))
                return true;
            return false;
        }

        private string CleanWhiteSpacesAndComments(string sDirty)
        {
            string sClean = "";
            for (int i = 0 ; i < sDirty.Length ; i++)
            {
                char c = sDirty[i];
                if (c == '/' && i < sDirty.Length - 1 && sDirty[i + 1] == '/') // this is a comment
                    return sClean;
                if (c > ' ' && c <= '~')//ignore white spaces
                    sClean += c;
            }
            return sClean;
        }


        private void InitCommandDictionaries()
        {
            m_dControl = new Dictionary<string, int[]>();

            m_dControl["0"] = new int[] { 0, 1, 0, 1, 0, 1, 0 };
            m_dControl["1"] = new int[] { 0, 1, 1, 1, 1, 1, 1 };
            m_dControl["-1"] = new int[] { 0, 1, 1, 1, 0, 1, 0 };
            m_dControl["D"] = new int[] { 0, 0, 0, 1, 1, 0, 0 };
            m_dControl["A"] = new int[] { 0, 1, 1, 0, 0, 0, 0 };
            m_dControl["!D"] = new int[] { 0, 0, 0, 1, 1, 0, 1 };
            m_dControl["!A"] = new int[] { 0, 1, 1, 0, 0, 0, 1 };
            m_dControl["-D"] = new int[] { 0, 0, 0, 1, 1, 1, 1 };
            m_dControl["-A"] = new int[] { 0, 1, 1, 0, 0,1, 1 };
            m_dControl["D+1"] = new int[] { 0, 0, 1, 1, 1, 1, 1 };
            m_dControl["A+1"] = new int[] { 0, 1, 1, 0, 1, 1, 1 };
            m_dControl["D-1"] = new int[] { 0, 0, 0, 1, 1, 1, 0 };
            m_dControl["A-1"] = new int[] { 0, 1, 1, 0, 0, 1, 0 };
            m_dControl["D+A"] = new int[] { 0, 0, 0, 0, 0, 1, 0 };
            m_dControl["D-A"] = new int[] { 0, 0, 1, 0, 0, 1, 1 };
            m_dControl["A-D"] = new int[] { 0, 0, 0, 0, 1,1, 1 };
            m_dControl["D&A"] = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            m_dControl["D|A"] = new int[] { 0, 0, 1, 0,1, 0, 1 };

            m_dControl["M"] = new int[] { 1, 1, 1, 0, 0, 0, 0 };
            m_dControl["!M"] = new int[] { 1, 1, 1, 0, 0, 0, 1 };
            m_dControl["-M"] = new int[] { 1, 1, 1, 0, 0, 1, 1 };
            m_dControl["M+1"] = new int[] { 1, 1, 1, 0, 1, 1, 1 };
            m_dControl["M-1"] = new int[] { 1, 1, 1, 0, 0, 1, 0 };
            m_dControl["D+M"] = new int[] { 1, 0, 0, 0, 0, 1, 0 };
            m_dControl["D-M"] = new int[] { 1, 0, 1, 0, 0, 1, 1 };
            m_dControl["M-D"] = new int[] { 1, 0, 0, 0, 1, 1, 1 };
            m_dControl["D&M"] = new int[] { 1, 0, 0, 0, 0, 0, 0 };
            m_dControl["D|M"] = new int[] { 1, 0, 1, 0, 1, 0, 1 };
            m_dControl["M+D"] = new int[] { 1, 0, 0, 0, 0, 1, 0 };


            m_dJmp = new Dictionary<string, int[]>();

            m_dJmp[""] = new int[] { 0, 0, 0 };
            m_dJmp["JGT"] = new int[] { 0, 0, 1 };
            m_dJmp["JEQ"] = new int[] { 0, 1, 0 };
            m_dJmp["JGE"] = new int[] { 0, 1, 1 };
            m_dJmp["JLT"] = new int[] { 1, 0, 0 };
            m_dJmp["JNE"] = new int[] { 1, 0, 1 };
            m_dJmp["JLE"] = new int[] { 1, 1, 0 };
            m_dJmp["JMP"] = new int[] { 1, 1, 1 };
        }
    }
}
