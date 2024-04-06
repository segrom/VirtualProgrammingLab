
window.codeFunctions = {
    addCodeAreaProvider: function () {
        codeInput.registerTemplate("syntax-highlighted", codeInput.templates.prism(Prism, [
            new codeInput.plugins.Indent(true, 4), // 2 spaces indentation
            new codeInput.plugins.AutoCloseBrackets(),
            new codeInput.plugins.Autocomplete()
        ]));

        let codeArea = document.querySelectorAll('code-input > textarea').item(0)
        let codeProvider = document.querySelector("#code-provider");

        codeArea.addEventListener('change', function () {
            codeProvider.value = codeArea.value;
            codeProvider.dispatchEvent(new Event('change'));
        })
        codeArea.addEventListener('keyup', function () {
            codeProvider.value = codeArea.value;
            codeProvider.dispatchEvent(new Event('change'));
        })
    }
}
