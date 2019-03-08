using System.Globalization;
using NUnit.Framework;
using UnityEditor;
using UDebug = UnityEngine.Debug;
using lieutenantgames.listparser;

namespace lieutenantgames.listparser.Tests {
    public class ListParserTests {
        [Test]
        public void generalTests() {
            ListOrValue listOrVal;
            TokenPos[] tokens;
            string str;

            // null should return default value
            listOrVal = LispParser.fromLisp(null);
            Assert.AreEqual(default(ListOrValue), listOrVal);

            // zero length string should return default value
            listOrVal = LispParser.fromLisp("");
            Assert.AreEqual(default(ListOrValue), listOrVal);

            // non zero lenght string composed only of chars < 33 should return default value
            listOrVal = LispParser.fromLisp(" ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" \t");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" \t\r");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" \t\r\n");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            // non zero lenght string with at least one char > 32 should return list of members if no syntax error ocurred
            listOrVal = LispParser.fromLisp("a");
            Assert.AreEqual("a", listOrVal[0].extract());
            listOrVal = LispParser.fromLisp(" a");
            Assert.AreEqual("a", listOrVal[0].extract());
            listOrVal = LispParser.fromLisp("a ");
            Assert.AreEqual("a", listOrVal[0].extract());
            listOrVal = LispParser.fromLisp(" a ");
            Assert.AreEqual("a", listOrVal[0].extract());

            listOrVal = LispParser.fromLisp("()");
            Assert.AreEqual(0, listOrVal[0].count());
            listOrVal = LispParser.fromLisp(" ()");
            Assert.AreEqual(0, listOrVal[0].count());
            listOrVal = LispParser.fromLisp("() ");
            Assert.AreEqual(0, listOrVal[0].count());
            listOrVal = LispParser.fromLisp(" () ");
            Assert.AreEqual(0, listOrVal[0].count());

            listOrVal = LispParser.fromLisp("a b");
            Assert.AreEqual(2, listOrVal.count());
            listOrVal = LispParser.fromLisp("a b c");
            Assert.AreEqual(3, listOrVal.count());

            // syntax errors
            // non matching open/close parens should return default
            listOrVal = LispParser.fromLisp("(");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" (");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp("( ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" ( ");
            Assert.AreEqual(default(ListOrValue), listOrVal);

            listOrVal = LispParser.fromLisp(")");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" )");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(") ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" ) ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            // non matching open/close string should return default
            listOrVal = LispParser.fromLisp("\"");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" \"");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp("\" ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = LispParser.fromLisp(" \" ");
            Assert.AreEqual(default(ListOrValue), listOrVal);

            //

            listOrVal = LispParser.fromLisp("(asdf)");
            Assert.AreEqual("asdf", listOrVal[0][0].extract());
            Assert.AreEqual(0, listOrVal.getTokenStart());
            Assert.AreEqual(6, listOrVal.getTokenEnd());
            Assert.AreEqual(0, listOrVal[0].getTokenStart());
            Assert.AreEqual(6, listOrVal[0].getTokenEnd());
            Assert.AreEqual(1, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(5, listOrVal[0][0].getTokenEnd());
            Assert.AreEqual(true, listOrVal[0].isList());
            Assert.GreaterOrEqual(listOrVal[0].count(), 0);
            Assert.AreEqual(1, listOrVal[0].count());
            Assert.AreEqual(1, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(5, listOrVal[0][0].getTokenEnd());

            listOrVal = LispParser.fromLisp("(asdf qwer)");
            Assert.AreEqual(0, listOrVal[0].getTokenStart());
            Assert.AreEqual(11, listOrVal[0].getTokenEnd());
            Assert.AreEqual(true, listOrVal[0].isList());
            Assert.GreaterOrEqual(listOrVal[0].count(), 0);
            Assert.AreEqual(2, listOrVal[0].count());
            Assert.AreEqual(1, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(5, listOrVal[0][0].getTokenEnd());
            Assert.AreEqual(6, listOrVal[0][1].getTokenStart());
            Assert.AreEqual(10, listOrVal[0][1].getTokenEnd());

            listOrVal = LispParser.fromLisp("(asdf \" qwer \")");
            Assert.AreEqual(0, listOrVal[0].getTokenStart());
            Assert.AreEqual(15, listOrVal[0].getTokenEnd());
            Assert.AreEqual(true, listOrVal[0].isList());
            Assert.GreaterOrEqual(listOrVal[0].count(), 0);
            Assert.AreEqual(2, listOrVal[0].count());
            Assert.AreEqual(1, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(5, listOrVal[0][0].getTokenEnd());
            Assert.AreEqual(6, listOrVal[0][1].getTokenStart());
            Assert.AreEqual(14, listOrVal[0][1].getTokenEnd());

            str = "(a(b c)d)";
            listOrVal = LispParser.fromLisp(str);
            Assert.AreEqual(0, listOrVal[0].getTokenStart());
            Assert.AreEqual(9, listOrVal[0].getTokenEnd());
            Assert.AreEqual(true, listOrVal[0].isList());
            Assert.GreaterOrEqual(listOrVal[0].count(), 0);
            Assert.AreEqual(3, listOrVal[0].count());
            Assert.AreEqual(false, listOrVal[0][0].isList());
            Assert.Less(listOrVal[0][0].count(), 0);
            Assert.AreEqual(true, listOrVal[0][1].isList());
            Assert.GreaterOrEqual(listOrVal[0][1].count(), 0);
            Assert.AreEqual(false, listOrVal[0][2].isList());
            Assert.Less(listOrVal[0][2].count(), 0);

            Assert.AreEqual(1, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(2, listOrVal[0][0].getTokenEnd());
            Assert.AreEqual(2, listOrVal[0][1].getTokenStart());
            Assert.AreEqual(7, listOrVal[0][1].getTokenEnd());
            Assert.AreEqual(7, listOrVal[0][2].getTokenStart());
            Assert.AreEqual(8, listOrVal[0][2].getTokenEnd());

            Assert.AreEqual(2, listOrVal[0][1].count());
            Assert.AreEqual(false, listOrVal[0][1][0].isList());
            Assert.Less(listOrVal[0][1][0].count(), 0);
            Assert.AreEqual(false, listOrVal[0][1][1].isList());
            Assert.Less(listOrVal[0][1][1].count(), 0);
            Assert.AreEqual(3, listOrVal[0][1][0].getTokenStart());
            Assert.AreEqual(4, listOrVal[0][1][0].getTokenEnd());
            Assert.AreEqual(5, listOrVal[0][1][1].getTokenStart());
            Assert.AreEqual(6, listOrVal[0][1][1].getTokenEnd());

            str = " ( a ( b c ) d ) ";
            listOrVal = LispParser.fromLisp(str);
            Assert.AreEqual(1, listOrVal[0].getTokenStart());
            Assert.AreEqual(16, listOrVal[0].getTokenEnd());
            Assert.AreEqual(true, listOrVal[0].isList());
            Assert.GreaterOrEqual(listOrVal[0].count(), 0);
            Assert.AreEqual(3, listOrVal[0].count());
            Assert.AreEqual(false, listOrVal[0][0].isList());
            Assert.Less(listOrVal[0][0].count(), 0);
            Assert.AreEqual(true, listOrVal[0][1].isList());
            Assert.GreaterOrEqual(listOrVal[0][1].count(), 0);
            Assert.AreEqual(false, listOrVal[0][2].isList());
            Assert.Less(listOrVal[0][2].count(), 0);

            Assert.AreEqual(3, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(4, listOrVal[0][0].getTokenEnd());
            Assert.AreEqual(5, listOrVal[0][1].getTokenStart());
            Assert.AreEqual(12, listOrVal[0][1].getTokenEnd());
            Assert.AreEqual(13, listOrVal[0][2].getTokenStart());
            Assert.AreEqual(14, listOrVal[0][2].getTokenEnd());

            Assert.AreEqual(2, listOrVal[0][1].count());
            Assert.AreEqual(false, listOrVal[0][1][0].isList());
            Assert.Less(listOrVal[0][1][0].count(), 0);
            Assert.AreEqual(false, listOrVal[0][1][1].isList());
            Assert.Less(listOrVal[0][1][1].count(), 0);
            Assert.AreEqual(7, listOrVal[0][1][0].getTokenStart());
            Assert.AreEqual(8, listOrVal[0][1][0].getTokenEnd());
            Assert.AreEqual(9, listOrVal[0][1][1].getTokenStart());
            Assert.AreEqual(10, listOrVal[0][1][1].getTokenEnd());

            str = "(a (b c) d)";
            listOrVal = LispParser.fromLisp(str);
            Assert.AreEqual(0, listOrVal[0].getTokenStart());
            Assert.AreEqual(11, listOrVal[0].getTokenEnd());
            Assert.AreEqual(true, listOrVal[0].isList());
            Assert.GreaterOrEqual(listOrVal[0].count(), 0);
            Assert.AreEqual(3, listOrVal[0].count());
            Assert.AreEqual(false, listOrVal[0][0].isList());
            Assert.Less(listOrVal[0][0].count(), 0);
            Assert.AreEqual(true, listOrVal[0][1].isList());
            Assert.GreaterOrEqual(listOrVal[0][1].count(), 0);
            Assert.AreEqual(false, listOrVal[0][2].isList());
            Assert.Less(listOrVal[0][2].count(), 0);

            Assert.AreEqual(1, listOrVal[0][0].getTokenStart());
            Assert.AreEqual(2, listOrVal[0][0].getTokenEnd());
            Assert.AreEqual(3, listOrVal[0][1].getTokenStart());
            Assert.AreEqual(8, listOrVal[0][1].getTokenEnd());
            Assert.AreEqual(9, listOrVal[0][2].getTokenStart());
            Assert.AreEqual(10, listOrVal[0][2].getTokenEnd());

            Assert.AreEqual(2, listOrVal[0][1].count());
            Assert.AreEqual(false, listOrVal[0][1][0].isList());
            Assert.Less(listOrVal[0][1][0].count(), 0);
            Assert.AreEqual(false, listOrVal[0][1][1].isList());
            Assert.Less(listOrVal[0][1][1].count(), 0);
            Assert.AreEqual(4, listOrVal[0][1][0].getTokenStart());
            Assert.AreEqual(5, listOrVal[0][1][0].getTokenEnd());
            Assert.AreEqual(6, listOrVal[0][1][1].getTokenStart());
            Assert.AreEqual(7, listOrVal[0][1][1].getTokenEnd());

            str = "(\"\" () a (c (e (g (i (\"\" () \"\") j) h) f) d) b)";
            listOrVal = LispParser.fromLisp(str);
            // EditorGUIUtility.systemCopyBuffer = listOrVal.getByIdx (0].toLisp (true);
            // UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            str = "(\"\" () a (c (e (g (i (\"\" () \"\") j) h) f) d) b)";
            listOrVal = LispParser.fromLisp(str);
            // EditorGUIUtility.systemCopyBuffer = listOrVal.getByIdx (0].toLisp (true, 0, false, 4);
            // UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            str = "(eu  tu  ele  (nos vos eles)  era  uma  vez  (um (negocio legal) (negocio) negocio sensacional)  acabou  de  verdade)";
            listOrVal = LispParser.fromLisp(str);
            // EditorGUIUtility.systemCopyBuffer = listOrVal.getByIdx (0].toLisp (true);
            // UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            // string not containing " should extract unquoted
            // string not containing '(', ')' and ' ' should add unquoted
            // string containing " should extract quoted
        }

        [Test]
        public void toLispAndBackTests() {
            object input;
            string inputStr;
            string lisp;
            ListOrValue output;

            input = (int)1;
            inputStr = input.ToString();
            lisp = LispParser.toLisp(input).ToString();
            output = LispParser.fromLisp(lisp);
            Assert.AreEqual(true, output.isList());
            Assert.AreEqual(false, output[0].isList());
            Assert.AreEqual(inputStr, output[0].extract());

            input = new int[] { 1, 2, 3 };
            inputStr = input.ToString();
            lisp = LispParser.toLisp(input).ToString();
            output = LispParser.fromLisp(lisp);
            Assert.AreEqual(true, output.isList()); // the document list
            Assert.AreEqual(true, output[0].isList()); // the actual list
            Assert.AreEqual(true, output[0][0].isList()); // the dimensions list
            Assert.AreEqual(1, output[0][0].count()); // the dimensions list
            Assert.AreEqual("3", output[0][0][0].extract()); // the array length
            Assert.AreEqual("1", output[0][1].extract()); // the actual element
            Assert.AreEqual("2", output[0][2].extract()); // the actual element
            Assert.AreEqual("3", output[0][3].extract()); // the actual element

            input = new int[,] { { 1 }, { 2 } };
            inputStr = input.ToString();
            lisp = LispParser.toLisp(input).ToString();
            output = LispParser.fromLisp(lisp);
            Assert.AreEqual(true, output.isList()); // the document list
            Assert.AreEqual(true, output[0].isList()); // the actual list
            Assert.AreEqual(true, output[0][0].isList()); // the dimensions list
            Assert.AreEqual(2, output[0][0].count()); // the dimensions list
            Assert.AreEqual("2", output[0][0][0].extract()); // the array length
            Assert.AreEqual("1", output[0][0][1].extract()); // the array length
            Assert.AreEqual("1", output[0][1].extract()); // the actual element
            Assert.AreEqual("2", output[0][2].extract()); // the actual element

            input = (float)1;
            inputStr = ((float)input).ToString(CultureInfo.InvariantCulture);
            lisp = LispParser.toLisp(input).ToString();
            output = LispParser.fromLisp(lisp);
            Assert.AreEqual(true, output.isList());
            Assert.AreEqual(false, output[0].isList());
            Assert.AreEqual("1", output[0].extract());


        }
    }
}
