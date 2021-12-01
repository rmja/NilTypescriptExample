using NiL.JS;
using NiL.JS.Extensions;
using System.Diagnostics;

var script = GetTypescriptServicesScript();
var module = new Module("typescriptServices.js", script);
module.Run();

var transpileFunction = module.Context.Eval(@"
input => {
    const result = ts.transpileModule(input, {
        compilerOptions: {
            target: ts.ScriptTarget.ES2015,
            module: ts.ModuleKind.ES2015,
            lib: [ 'ES2015' ]
        }
    });
    return result.outputText;
};
");

var tsc = transpileFunction.As<NiL.JS.BaseLibrary.Function>().MakeDelegate<Func<string, string>>();



var typescriptSource = @"
import { SampleStore } from 'utiliread';

export default class Test {
    static inject = [SampleStore];
    constructor(private ss: SampleStore) {
    }
}";

var transpiled = tsc(typescriptSource);

Debug.Assert(@"import { SampleStore } from 'utiliread';
export default class Test {
    constructor(ss) {
        this.ss = ss;
    }
}
Test.inject = [SampleStore];
" == transpiled);


static Script GetTypescriptServicesScript()
{
    // https://raw.githubusercontent.com/microsoft/TypeScript/main/lib/typescriptServices.js
    using var stream = typeof(Program).Assembly.GetManifestResourceStream("NilTypescriptExample.typescriptServices.js")!;
    using var reader = new StreamReader(stream);
    var code = reader.ReadToEnd();
    code = "var globalThis = this;" + code;
    return Script.Parse(code);
}