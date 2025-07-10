

## 1.2.0

- Change the minimum required version of Serilog for JG.Core.Logging.Formatters to 2.12.0, allowing better compatibility with legacy services.
- Fix an issue with formatting the message when accessing the `StackTrace` on property an `Exception` instance throws an exception.

## 1.1.0

- Add `err.innerErrors` property, which contains the details of all inner exceptions.

## 1.0.0

Initial release
