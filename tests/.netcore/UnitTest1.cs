using lieutenantgames.listparser;
using NUnit.Framework;

namespace lieutenantgames.listparser.Tests {
    // TODO rename from ListParserTests to ListProcessorTests
    public class ListParserTests {
        [Test]
        public void generalTests() {

            ListOrValue listOrVal;
            TokenPos[] tokens;
            string str;

            // null should return default value
            listOrVal = ListParser.tokenizeAndParse(null);
            Assert.AreEqual(default(ListOrValue), listOrVal);

            // zero length string should return default value
            listOrVal = ListParser.tokenizeAndParse("");
            Assert.AreEqual(default(ListOrValue), listOrVal);

            // non zero lenght string composed only of chars < 33 should return default value
            listOrVal = ListParser.tokenizeAndParse(" ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" \t");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" \t\r");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" \t\r\n");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            // non zero lenght string with at least one char > 32 should return list of members if no syntax error ocurred
            listOrVal = ListParser.tokenizeAndParse("a");
            Assert.AreEqual("a", listOrVal.getByIdx(0).extract());
            listOrVal = ListParser.tokenizeAndParse(" a");
            Assert.AreEqual("a", listOrVal.getByIdx(0).extract());
            listOrVal = ListParser.tokenizeAndParse("a ");
            Assert.AreEqual("a", listOrVal.getByIdx(0).extract());
            listOrVal = ListParser.tokenizeAndParse(" a ");
            Assert.AreEqual("a", listOrVal.getByIdx(0).extract());

            listOrVal = ListParser.tokenizeAndParse("()");
            Assert.AreEqual(0, listOrVal.getByIdx(0).count());
            listOrVal = ListParser.tokenizeAndParse(" ()");
            Assert.AreEqual(0, listOrVal.getByIdx(0).count());
            listOrVal = ListParser.tokenizeAndParse("() ");
            Assert.AreEqual(0, listOrVal.getByIdx(0).count());
            listOrVal = ListParser.tokenizeAndParse(" () ");
            Assert.AreEqual(0, listOrVal.getByIdx(0).count());

            listOrVal = ListParser.tokenizeAndParse("a b");
            Assert.AreEqual(2, listOrVal.count());
            listOrVal = ListParser.tokenizeAndParse("a b c");
            Assert.AreEqual(3, listOrVal.count());

            // syntax errors
            // non matching open/close parens should return default
            listOrVal = ListParser.tokenizeAndParse("(");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" (");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse("( ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" ( ");
            Assert.AreEqual(default(ListOrValue), listOrVal);

            listOrVal = ListParser.tokenizeAndParse(")");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" )");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(") ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" ) ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            // non matching open/close string should return default
            listOrVal = ListParser.tokenizeAndParse("\"");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" \"");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse("\" ");
            Assert.AreEqual(default(ListOrValue), listOrVal);
            listOrVal = ListParser.tokenizeAndParse(" \" ");
            Assert.AreEqual(default(ListOrValue), listOrVal);

            //

            listOrVal = ListParser.tokenizeAndParse("(asdf)");
            Assert.AreEqual("asdf", listOrVal.getByIdx(0).getByIdx(0).extract());
            Assert.AreEqual(0, listOrVal.getByIdx(0).getTokenStart());
            Assert.AreEqual(6, listOrVal.getByIdx(0).getTokenStart());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).isList());
            Assert.AreEqual(1, listOrVal.getByIdx(0).count());
            Assert.AreEqual(1, listOrVal.getByIdx(0).getByIdx(0).getTokenStart());
            Assert.AreEqual(5, listOrVal.getByIdx(0).getByIdx(0).getTokenEnd());

            listOrVal = ListParser.tokenizeAndParse("(asdf qwer)");
            Assert.AreEqual(0, listOrVal.getByIdx(0).getTokenStart());
            Assert.AreEqual(11, listOrVal.getByIdx(0).getTokenEnd());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).isList());
            Assert.AreEqual(2, listOrVal.getByIdx(0).count());
            Assert.AreEqual(1, listOrVal.getByIdx(0).getByIdx(0).getTokenStart());
            Assert.AreEqual(5, listOrVal.getByIdx(0).getByIdx(0).getTokenEnd());
            Assert.AreEqual(6, listOrVal.getByIdx(0).getByIdx(1).getTokenStart());
            Assert.AreEqual(10, listOrVal.getByIdx(0).getByIdx(1).getTokenEnd());

            listOrVal = ListParser.tokenizeAndParse("(asdf \" qwer \")");
            Assert.AreEqual(0, listOrVal.getByIdx(0).getTokenStart());
            Assert.AreEqual(15, listOrVal.getByIdx(0).getTokenEnd());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).isList());
            Assert.AreEqual(2, listOrVal.getByIdx(0).count());
            Assert.AreEqual(1, listOrVal.getByIdx(0).getByIdx(0).getTokenStart());
            Assert.AreEqual(5, listOrVal.getByIdx(0).getByIdx(0).getTokenEnd());
            Assert.AreEqual(6, listOrVal.getByIdx(0).getByIdx(1).getTokenStart());
            Assert.AreEqual(14, listOrVal.getByIdx(0).getByIdx(1).getTokenEnd());

            str = "(a(b c)d)";
            listOrVal = ListParser.tokenizeAndParse(str);
            Assert.AreEqual(0, listOrVal.getByIdx(0).getTokenStart());
            Assert.AreEqual(9, listOrVal.getByIdx(0).getTokenEnd());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).isList());
            Assert.AreEqual(3, listOrVal.getByIdx(0).count());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(0).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(0).isList());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).getByIdx(1).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).getByIdx(1).isList());
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(2).isList());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(2).m_list);

            Assert.AreEqual(1, listOrVal.getByIdx(0).getByIdx(0).getTokenStart());
            Assert.AreEqual(2, listOrVal.getByIdx(0).getByIdx(0).getTokenEnd());
            Assert.AreEqual(2, listOrVal.getByIdx(0).getByIdx(1).getTokenStart());
            Assert.AreEqual(7, listOrVal.getByIdx(0).getByIdx(1).getTokenEnd());
            Assert.AreEqual(7, listOrVal.getByIdx(0).getByIdx(2).getTokenStart());
            Assert.AreEqual(8, listOrVal.getByIdx(0).getByIdx(2).getTokenEnd());

            Assert.AreEqual(2, listOrVal.getByIdx(0).getByIdx(1).count());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).isList());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).isList());
            Assert.AreEqual(3, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).getTokenStart());
            Assert.AreEqual(4, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).getTokenEnd());
            Assert.AreEqual(5, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).getTokenStart());
            Assert.AreEqual(6, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).getTokenEnd());

            str = " ( a ( b c ) d ) ";
            listOrVal = ListParser.tokenizeAndParse(str);
            Assert.AreEqual(1, listOrVal.getByIdx(0).getTokenStart());
            Assert.AreEqual(16, listOrVal.getByIdx(0).getTokenEnd());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).isList());
            Assert.AreEqual(3, listOrVal.getByIdx(0).count());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(0).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(0).isList());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).getByIdx(1).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).getByIdx(1).isList());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(2).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(2).isList());

            Assert.AreEqual(3, listOrVal.getByIdx(0).getByIdx(0).getTokenStart());
            Assert.AreEqual(4, listOrVal.getByIdx(0).getByIdx(0).getTokenEnd());
            Assert.AreEqual(5, listOrVal.getByIdx(0).getByIdx(1).getTokenStart());
            Assert.AreEqual(12, listOrVal.getByIdx(0).getByIdx(1).getTokenEnd());
            Assert.AreEqual(13, listOrVal.getByIdx(0).getByIdx(2).getTokenStart());
            Assert.AreEqual(14, listOrVal.getByIdx(0).getByIdx(2).getTokenEnd());

            Assert.AreEqual(2, listOrVal.getByIdx(0).getByIdx(1).count());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).isList());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).isList());
            Assert.AreEqual(7, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).getTokenStart());
            Assert.AreEqual(8, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).getTokenEnd());
            Assert.AreEqual(9, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).getTokenStart());
            Assert.AreEqual(10, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).getTokenEnd());

            str = "(a (b c) d)";
            listOrVal = ListParser.tokenizeAndParse(str);
            Assert.AreEqual(0, listOrVal.getByIdx(0).getTokenStart());
            Assert.AreEqual(11, listOrVal.getByIdx(0).getTokenEnd());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).isList());
            Assert.AreEqual(3, listOrVal.getByIdx(0).count());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(0).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).getByIdx(0).isList());
            // Assert.AreNotEqual (null, listOrVal.getByIdx(0).getByIdx(1).m_list);
            Assert.AreEqual(true, listOrVal.getByIdx(0).getByIdx(1).isList());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(2).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(2).isList());

            Assert.AreEqual(1, listOrVal.getByIdx(0).getByIdx(0).getTokenStart());
            Assert.AreEqual(2, listOrVal.getByIdx(0).getByIdx(0).getTokenEnd());
            Assert.AreEqual(3, listOrVal.getByIdx(0).getByIdx(1).getTokenStart());
            Assert.AreEqual(8, listOrVal.getByIdx(0).getByIdx(1).getTokenEnd());
            Assert.AreEqual(9, listOrVal.getByIdx(0).getByIdx(2).getTokenStart());
            Assert.AreEqual(10, listOrVal.getByIdx(0).getByIdx(2).getTokenEnd());

            Assert.AreEqual(2, listOrVal.getByIdx(0).getByIdx(1).count());
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).isList());
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).m_list);
            // Assert.AreEqual (null, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).m_list);
            Assert.AreEqual(false, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).isList());
            Assert.AreEqual(4, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).getTokenStart());
            Assert.AreEqual(5, listOrVal.getByIdx(0).getByIdx(1).getByIdx(0).getTokenEnd());
            Assert.AreEqual(6, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).getTokenStart());
            Assert.AreEqual(7, listOrVal.getByIdx(0).getByIdx(1).getByIdx(1).getTokenEnd());

            str = "(\"\" () a (c (e (g (i (\"\" () \"\") j) h) f) d) b)";
            listOrVal = ListParser.tokenizeAndParse(str);
            // EditorGUIUtility.systemCopyBuffer = listOrVal.getByIdx (0).toLisp (true);
            // UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            str = "(\"\" () a (c (e (g (i (\"\" () \"\") j) h) f) d) b)";
            listOrVal = ListParser.tokenizeAndParse(str);
            // EditorGUIUtility.systemCopyBuffer = listOrVal.getByIdx (0).toLisp (true, 0, false, 4);
            // UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            str = "(eu  tu  ele  (nos vos eles)  era  uma  vez  (um (negocio legal) (negocio) negocio sensacional)  acabou  de  verdade)";
            listOrVal = ListParser.tokenizeAndParse(str);
            // EditorGUIUtility.systemCopyBuffer = listOrVal.getByIdx (0).toLisp (true);
            // UDebug.Log (EditorGUIUtility.systemCopyBuffer);

            // string not containing " should extract unquoted
            // string not containing '(', ')' and ' ' should add unquoted
            // string containing " should extract quoted
        }
    }
}
