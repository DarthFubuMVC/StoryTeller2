﻿using System.Linq;
using Baseline;
using Shouldly;
using StoryTeller.Model;
using StoryTeller.Model.Persistence.DSL;
using Xunit;

namespace StoryTeller.Testing.Model.Persistence
{
    public class loading_fixture_with_markdown
    {
        [Fact]
        public void reads_title()
        {
            var result = FixtureReader.ReadFrom(@"# a title");
            result.title.ShouldBe("a title");
        }

        [Fact]
        public void can_derive_a_title_from_the_key()
        {
            var result = FixtureReader.ReadFrom(@"
## SomeKindOfKey
");
            result.grammars.Single().ShouldBeOfType<Sentence>()
                .format.ShouldBe("Some Kind Of Key");
        }

        [Fact]
        public void can_fix_up_a_key_with_spaces()
        {
            var result = FixtureReader.ReadFrom(@"
## Some Kind Of Key
");
            var sentence = result.grammars.Single().ShouldBeOfType<Sentence>();
            sentence
                .format.ShouldBe("Some Kind Of Key");

            sentence.key.ShouldBe("SomeKindOfKey");
        }

        [Fact]
        public void reads_sentence()
        {
            var result = FixtureReader.ReadFrom(@"
## AKey
### a title");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();
            result.grammars[0].As<Sentence>().format.ShouldBe("a title");
            result.grammars[0].As<Sentence>().key.ShouldBe("AKey");
        }

        [Fact]
        public void reads_sentence_cells()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
|sentence|first|
|default|something|");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.DefaultValue.ShouldBe("something");
        }

        [Fact]
        public void reads_sentence_cells_with_options()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
|sentence|first               |
|default |somthing            |
|options |hello, goodbye, ciao|");

            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.options.ShouldNotBeNull();
            cell.options.Length.ShouldBe(3);
            cell.options[0].value.ShouldBe("hello");
            cell.options[1].value.ShouldBe("goodbye");
            cell.options[2].value.ShouldBe("ciao");
        }

        [Fact(Skip = "not handled yet")]
        public void reads_sentence_cells_with_quoted_options()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|sentence|first                           |
|default |something                       |
|options |hello, ""goodbye, friend"", ciao|
");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.options.ShouldNotBeNull();
            cell.options.Length.ShouldBe(3);
            cell.options[0].value.ShouldBe("hello");
            cell.options[1].value.ShouldBe("goodbye, friend");
            cell.options[2].value.ShouldBe("ciao");
        }

        [Fact]
        public void reads_sentence_cells_with_editor()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|sentence|first   |
|editor  |bigtext |
");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.Editor.ShouldBe("bigtext");
        }

        [Fact]
        public void reads_sentence_cells_with_result_true()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|sentence|first|
|result  |true |
");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.IsResult.ShouldBeTrue();
        }

        [Fact]
        public void reads_sentence_cells_with_result_false()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|sentence|first|
|result  |false|
");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.IsResult.ShouldBeFalse();
        }

        [Fact]
        public void reads_sentence_cells_with_result_empty()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|sentence|first|
|result  |     |
");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Sentence>();

            var sentence = result.grammars[0].As<Sentence>();
            sentence.cells.ShouldNotBeNull();
            sentence.cells.Length.ShouldBe(1);

            var cell = sentence.cells[0];
            cell.Key.ShouldBe("first");
            cell.IsResult.ShouldBeFalse();
        }

        [Fact]
        public void reads_table()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|table|col1|");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Table>();

            var table = result.grammars[0].As<Table>();
            table.cells.ShouldNotBeNull();
            table.cells.Length.ShouldBe(1);

            var cell = table.cells[0];
            cell.Key.ShouldBe("col1");
        }

        [Fact]
        public void reads_table_default_value()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|table|col1|
|default|one|");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Table>();

            var table = result.grammars[0].As<Table>();
            table.cells.ShouldNotBeNull();
            table.cells.Length.ShouldBe(1);

            var cell = table.cells[0];
            cell.Key.ShouldBe("col1");
            cell.DefaultValue.ShouldBe("one");
        }

        [Fact]
        public void reads_table_multiple_columns()
        {
            var result = FixtureReader.ReadFrom(@"
## a key
### a title
|table|column 1|column 2|
|default|one|two|
|editor|text||");
            result.grammars.ShouldNotBeNull();
            result.grammars.Length.ShouldBe(1);
            result.grammars[0].ShouldBeOfType<Table>();

            var table = result.grammars[0].As<Table>();
            table.cells.ShouldNotBeNull();
            table.cells.Length.ShouldBe(2);

            var cell = table.cells[0];
            cell.Key.ShouldBe("column 1");
            cell.DefaultValue.ShouldBe("one");
            cell.Editor.ShouldBe("text");

            cell = table.cells[1];
            cell.Key.ShouldBe("column 2");
            cell.DefaultValue.ShouldBe("two");
        }
    }
}
