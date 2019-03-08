using NUnit.Framework;
using UnityEditor;
using UDebug = UnityEngine.Debug;

namespace lieutenantgames.listparser {
    // TODO rename from ListParserTests to ListProcessorTests
    public class ListParserTests {
        [Test]
        public void generalTests () {

            ListOrValue listOrVal;
            TokenPos[] tokens;
            string str;

            // null should return default value
            listOrVal = ListParser.tokenizeAndParse (null);
            Assert.AreEqual (default (ListOrValue), listOrVal);

            // zero length string should return default value
            listOrVal = ListParser.tokenizeAndParse ("");
            Assert.AreEqual (default (ListOrValue), listOrVal);

            // non zero lenght string composed only of chars < 33 should return default value
            listOrVal = ListParser.tokenizeAndParse (" ");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" \t");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" \t\r");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" \t\r\n");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            // non zero lenght string with at least one char > 32 should return list of members if no syntax error ocurred
            listOrVal = ListParser.tokenizeAndParse ("a");
            Assert.AreEqual ("a", listOrVal.m_list[0].m_value.extract ());
            listOrVal = ListParser.tokenizeAndParse (" a");
            Assert.AreEqual ("a", listOrVal.m_list[0].m_value.extract ());
            listOrVal = ListParser.tokenizeAndParse ("a ");
            Assert.AreEqual ("a", listOrVal.m_list[0].m_value.extract ());
            listOrVal = ListParser.tokenizeAndParse (" a ");
            Assert.AreEqual ("a", listOrVal.m_list[0].m_value.extract ());

            listOrVal = ListParser.tokenizeAndParse ("()");
            Assert.AreEqual (0, listOrVal.m_list[0].m_list.Count);
            listOrVal = ListParser.tokenizeAndParse (" ()");
            Assert.AreEqual (0, listOrVal.m_list[0].m_list.Count);
            listOrVal = ListParser.tokenizeAndParse ("() ");
            Assert.AreEqual (0, listOrVal.m_list[0].m_list.Count);
            listOrVal = ListParser.tokenizeAndParse (" () ");
            Assert.AreEqual (0, listOrVal.m_list[0].m_list.Count);

            listOrVal = ListParser.tokenizeAndParse ("a b");
            Assert.AreEqual (2, listOrVal.m_list.Count);
            listOrVal = ListParser.tokenizeAndParse ("a b c");
            Assert.AreEqual (3, listOrVal.m_list.Count);

            // syntax errors
            // non matching open/close parens should return default
            listOrVal = ListParser.tokenizeAndParse ("(");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" (");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse ("( ");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" ( ");
            Assert.AreEqual (default (ListOrValue), listOrVal);

            listOrVal = ListParser.tokenizeAndParse (")");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" )");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (") ");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" ) ");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            // non matching open/close string should return default
            listOrVal = ListParser.tokenizeAndParse ("\"");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" \"");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse ("\" ");
            Assert.AreEqual (default (ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse (" \" ");
            Assert.AreEqual (default (ListOrValue), listOrVal);

            //

            listOrVal = ListParser.tokenizeAndParse ("(asdf)");
            Assert.AreEqual ("asdf", listOrVal.m_list[0].m_list[0].m_value.extract ());
            Assert.AreEqual (0, listOrVal.m_list[0].m_value.m_start);
            Assert.AreEqual (6, listOrVal.m_list[0].m_value.m_end);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list);
            Assert.AreEqual (1, listOrVal.m_list[0].m_list.Count);
            Assert.AreEqual (1, listOrVal.m_list[0].m_list[0].m_value.m_start);
            Assert.AreEqual (5, listOrVal.m_list[0].m_list[0].m_value.m_end);

            listOrVal = ListParser.tokenizeAndParse ("(asdf qwer)");
            Assert.AreEqual (0, listOrVal.m_list[0].m_value.m_start);
            Assert.AreEqual (11, listOrVal.m_list[0].m_value.m_end);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list);
            Assert.AreEqual (2, listOrVal.m_list[0].m_list.Count);
            Assert.AreEqual (1, listOrVal.m_list[0].m_list[0].m_value.m_start);
            Assert.AreEqual (5, listOrVal.m_list[0].m_list[0].m_value.m_end);
            Assert.AreEqual (6, listOrVal.m_list[0].m_list[1].m_value.m_start);
            Assert.AreEqual (10, listOrVal.m_list[0].m_list[1].m_value.m_end);

            listOrVal = ListParser.tokenizeAndParse ("(asdf \" qwer \")");
            Assert.AreEqual (0, listOrVal.m_list[0].m_value.m_start);
            Assert.AreEqual (15, listOrVal.m_list[0].m_value.m_end);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list);
            Assert.AreEqual (2, listOrVal.m_list[0].m_list.Count);
            Assert.AreEqual (1, listOrVal.m_list[0].m_list[0].m_value.m_start);
            Assert.AreEqual (5, listOrVal.m_list[0].m_list[0].m_value.m_end);
            Assert.AreEqual (6, listOrVal.m_list[0].m_list[1].m_value.m_start);
            Assert.AreEqual (14, listOrVal.m_list[0].m_list[1].m_value.m_end);


            str = "(a(b c)d)";
            listOrVal = ListParser.tokenizeAndParse (str);
            Assert.AreEqual (0, listOrVal.m_list[0].m_value.m_start);
            Assert.AreEqual (9, listOrVal.m_list[0].m_value.m_end);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list);
            Assert.AreEqual (3, listOrVal.m_list[0].m_list.Count);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[0].m_list);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list[1].m_list);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[2].m_list);

            Assert.AreEqual (1, listOrVal.m_list[0].m_list[0].m_value.m_start);
            Assert.AreEqual (2, listOrVal.m_list[0].m_list[0].m_value.m_end);
            Assert.AreEqual (2, listOrVal.m_list[0].m_list[1].m_value.m_start);
            Assert.AreEqual (7, listOrVal.m_list[0].m_list[1].m_value.m_end);
            Assert.AreEqual (7, listOrVal.m_list[0].m_list[2].m_value.m_start);
            Assert.AreEqual (8, listOrVal.m_list[0].m_list[2].m_value.m_end);

            Assert.AreEqual (2, listOrVal.m_list[0].m_list[1].m_list.Count);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[1].m_list[0].m_list);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[1].m_list[1].m_list);
            Assert.AreEqual (3, listOrVal.m_list[0].m_list[1].m_list[0].m_value.m_start);
            Assert.AreEqual (4, listOrVal.m_list[0].m_list[1].m_list[0].m_value.m_end);
            Assert.AreEqual (5, listOrVal.m_list[0].m_list[1].m_list[1].m_value.m_start);
            Assert.AreEqual (6, listOrVal.m_list[0].m_list[1].m_list[1].m_value.m_end);

            str = " ( a ( b c ) d ) ";
            listOrVal = ListParser.tokenizeAndParse (str);
            Assert.AreEqual (1, listOrVal.m_list[0].m_value.m_start);
            Assert.AreEqual (16, listOrVal.m_list[0].m_value.m_end);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list);
            Assert.AreEqual (3, listOrVal.m_list[0].m_list.Count);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[0].m_list);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list[1].m_list);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[2].m_list);

            Assert.AreEqual (3, listOrVal.m_list[0].m_list[0].m_value.m_start);
            Assert.AreEqual (4, listOrVal.m_list[0].m_list[0].m_value.m_end);
            Assert.AreEqual (5, listOrVal.m_list[0].m_list[1].m_value.m_start);
            Assert.AreEqual (12, listOrVal.m_list[0].m_list[1].m_value.m_end);
            Assert.AreEqual (13, listOrVal.m_list[0].m_list[2].m_value.m_start);
            Assert.AreEqual (14, listOrVal.m_list[0].m_list[2].m_value.m_end);

            Assert.AreEqual (2, listOrVal.m_list[0].m_list[1].m_list.Count);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[1].m_list[0].m_list);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[1].m_list[1].m_list);
            Assert.AreEqual (7, listOrVal.m_list[0].m_list[1].m_list[0].m_value.m_start);
            Assert.AreEqual (8, listOrVal.m_list[0].m_list[1].m_list[0].m_value.m_end);
            Assert.AreEqual (9, listOrVal.m_list[0].m_list[1].m_list[1].m_value.m_start);
            Assert.AreEqual (10, listOrVal.m_list[0].m_list[1].m_list[1].m_value.m_end);

            str = "(a (b c) d)";
            listOrVal = ListParser.tokenizeAndParse (str);
            Assert.AreEqual (0, listOrVal.m_list[0].m_value.m_start);
            Assert.AreEqual (11, listOrVal.m_list[0].m_value.m_end);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list);
            Assert.AreEqual (3, listOrVal.m_list[0].m_list.Count);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[0].m_list);
            Assert.AreNotEqual (null, listOrVal.m_list[0].m_list[1].m_list);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[2].m_list);

            Assert.AreEqual (1, listOrVal.m_list[0].m_list[0].m_value.m_start);
            Assert.AreEqual (2, listOrVal.m_list[0].m_list[0].m_value.m_end);
            Assert.AreEqual (3, listOrVal.m_list[0].m_list[1].m_value.m_start);
            Assert.AreEqual (8, listOrVal.m_list[0].m_list[1].m_value.m_end);
            Assert.AreEqual (9, listOrVal.m_list[0].m_list[2].m_value.m_start);
            Assert.AreEqual (10, listOrVal.m_list[0].m_list[2].m_value.m_end);

            Assert.AreEqual (2, listOrVal.m_list[0].m_list[1].m_list.Count);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[1].m_list[0].m_list);
            Assert.AreEqual (null, listOrVal.m_list[0].m_list[1].m_list[1].m_list);
            Assert.AreEqual (4, listOrVal.m_list[0].m_list[1].m_list[0].m_value.m_start);
            Assert.AreEqual (5, listOrVal.m_list[0].m_list[1].m_list[0].m_value.m_end);
            Assert.AreEqual (6, listOrVal.m_list[0].m_list[1].m_list[1].m_value.m_start);
            Assert.AreEqual (7, listOrVal.m_list[0].m_list[1].m_list[1].m_value.m_end);

            str = "(\"\" () a (c (e (g (i (\"\" () \"\") j) h) f) d) b)";
            listOrVal = ListParser.tokenizeAndParse (str);
            EditorGUIUtility.systemCopyBuffer = listOrVal.m_list[0].toLisp (true);
            UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            str = "(\"\" () a (c (e (g (i (\"\" () \"\") j) h) f) d) b)";
            listOrVal = ListParser.tokenizeAndParse (str);
            EditorGUIUtility.systemCopyBuffer = listOrVal.m_list[0].toLisp (true, 0, false, 4);
            UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            str = "(eu  tu  ele  (nos vos eles)  era  uma  vez  (um (negocio legal) (negocio) negocio sensacional)  acabou  de  verdade)";
            listOrVal = ListParser.tokenizeAndParse (str);
            EditorGUIUtility.systemCopyBuffer = listOrVal.m_list[0].toLisp (true);
            UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            // string not containing " should extract unquoted
            // string not containing '(', ')' and ' ' should add unquoted
            // string containing " should extract quoted
        }
    }
}
