include $(root_dir)/Makefile.include
ASSEMBLY_NAME=mindtouch.deki.script.tests
ASSEMBLY=$(ASSEMBLY_NAME).dll
TARGET=$(addprefix $(build_prefix)/, $(ASSEMBLY))

sources = \
                AssemblyInfo.cs \
                ScriptTests\DekiScriptLibraryListTests.cs   \
                ScriptTests\DekiScriptLibraryWebTests.cs    \
                ScriptTests\DekiScriptParserTests.cs		\
                ScriptTests\DekiScriptTester.cs             \
                ScriptTests\ExecutionTests.cs				\
                ScriptTests\ExpressionSerializationTests.cs	\
                ScriptTests\ExtensionTests.cs				\
                ScriptTests\NativeFunctionInvocationTests.cs    \
                ScriptTests\ScriptValidatorTests.cs			\
                ScriptTests\TestDekiScriptRuntime.cs    \
                ScriptTests\XmlNodePlainTextReadonlyByteStreamTests.cs	\
                DateTimeTests.cs	\
                SimpleHtmlFormatterTests.cs			\
                XhtmlValidatorTests.cs	\

references = \
                -r:System \
                -r:System.Data \
                -r:System.Xml \
                -r:System.Configuration \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.core.dll) \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.deki.ext.dll) \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.deki.script.dll) \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.deki.script.check.dll) \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.deki.services.dll) \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.dream.dll) \
                -r:$(addprefix $(root_dir)/, redist/mindtouch.dream.test.dll) \
                -r:$(addprefix $(root_dir)/, redist/nunit/nunit.framework.dll) \
                -r:$(addprefix $(root_dir)/, redist/log4net.dll)    \
                -r:$(addprefix $(root_dir)/, redist/SgmlReaderDll.dll)  \
                -r:$(addprefix $(root_dir)/, redist/Lucene.Net.dll) 

$(TARGET): 
	$(MCS) $(MCS_FLAGS) /out:$(TARGET) /target:library $(references) $(sources) 

all: $(TARGET)
