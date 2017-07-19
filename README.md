# sppv-migration
SharePoint personal views migration

Migrating SharePoint personal views from one to another environment using export and import model as described below.

Export

The export operation will export all the personal views in the site specified by the URL into the xml file.

Syntax:
SPPersonalViewMigrate.exe -o export -url <site colletion or site url> -file <export file>

Note:
If site collection's URL is specified then personal views of the whole site collection will be exported, otherwise single site's personal views will be exported.

Example:

SPPersonalViewMigrate.exe -o export –url "http://sharepoint/sites/mysite" -file "c:\export\mysite.xml"
SPPersonalViewMigrate.exe -o export –url "http://sharepoint/sites/mysite/subsite" -file "c:\export\subsite.xml"


Import

While the import operation allows personal views to be imported from one site to another site it also enables a single view of a certain user to be imported to the target list/library.

Syntax:

SPPersonalViewMigrate.exe -o import
-file <absolute path to the export file>
-source <site relative url>
-target <absolute url>
[-view <view name>]
[-login <login name>]
[-excludeLogin <login names to exlcude separated by comma>]
[-schemaPlainText]


Source: The site relative URL of the site from which personal views is imported

Target: The absolute URL of the site to which personal views is imported.

View: Specify a single view to be imported

Login: Specify the Login name whose view to be imported

ExcludeLogin: List of user to be excluded from the import, separated by comma

SchemaPlainText: Indicate whether the xml schema of the view is plain text which is true to views in SharePoint 2007 or previous.

Example:

SPPersonalViewMigrate.exe -o import -file "c:\export\mysite.xml" -source "sites/mysite/subsite" -target "http://sharepoint/sites/mysite/subsite" -schemaPlainText
