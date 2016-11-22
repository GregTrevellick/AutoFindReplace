# Road Map

- [ ] Improvements
   - [ ] Add a count of diabled rules to the Results dialog
   - [ ] Mark new rules as Enabled by default
   - [ ] Display warning if user chooses to allow modifications to files not under source-control
   - [ ] Add an option to allow user to supprerss the Results dialog if no modifications were made
   - [ ] Allow sorting of Rules

- [ ] Support for additional VS versions
   - [ ] Publish the extension for VS2005 
   - [ ] Publish the extension for VS2008 
   - [ ] Publish the extension for VS2010 
   - [ ] Publish the extension for VS15

- [ ] Convert codebase to .Net Core

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 1.0

**2016-10-29** <!--17:30 UK / 16:30 UTC-->

- [x] Initial release

## 1.1

**2016-11-05** <!--14:20 UK / 14:20 UTC-->

- [x] De-duplicate and alpha-sort successfull Results dialog messages
- [x] Minor cosmetic enhancements to Options and Results dialog
- [x] Bug fixes
  - [x] Results dialog not appearing under certain circumstances when rules for multiple solutions exist
  - [x] Exporting rules as CSV crashes if no rules exist
  - [x] Crash when a non-text file specified in a rule

## 1.2.1

**2016-11-09** <!--18:15 UK / 18:15 UTC-->

- [x] Publish the extension for VS2013
- [x] Publish the extension for VS2012

## 1.2.19

**2016-11-14** <!--08:00 UK / 08:00 UTC-->

- [x] Bug fix
  - [x] GitHub issue [#1](https://github.com/GregTrevellick/AutoFindReplace/issues/1) - Files not auto changed when outside the root of the solution file

## Coming soon...

- [x] Bug fix
  - [x] GitHub issue [#2](https://github.com/GregTrevellick/AutoFindReplace/issues/2) - Complex folder structures result in error "Path is not a legal form"

<!--travis //travis-ci.org/getting_started //docs.travis-ci.com/user/languages/csharp/ //github.com/mwrock/PowerShell/blob/master/.travis.yml-->

<!--sign the extension - not required for auto updates ! activity log ? github.com/mwrock/PowerShell/tree/master/src/signing-->

<!--rules kick in when opening a file not just a sln-->
<!--rules kick in when creating a sln-->
<!--auto column widths-->
<!--commas in csv-->
<!--suppress pop up if nowt changed-->
<!--"a" -> ""-->
<!--"a" -> "a"-->