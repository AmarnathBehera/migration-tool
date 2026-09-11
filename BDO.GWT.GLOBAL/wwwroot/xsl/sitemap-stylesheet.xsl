<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:s="http://www.sitemaps.org/schemas/sitemap/0.9"
    xmlns:xhtml="http://www.w3.org/1999/xhtml">
    <xsl:output method="html" indent="yes" encoding="utf-8"/>

    <xsl:template match="/">
        <html>
            <head>
                <title>Sitemap</title>
                <style type="text/css">
                    body {font-family: Helvetica, Arial, sans-serif; font-size: 13px; color: #000;}
                    table {border: none; border-collapse: collapse;}
                    #sitemap tr:nth-child(odd) td { background-color: #eee;}
                    #sitemap tbody tr:hover td { background-color: #ccc;}
                    #content { margin: 0 auto; width: 90%;}
                    a { color: #000; text-decoration: none;}
                    a:visited { color: #777;}
                    a:hover { text-decoration: underline;}
                    td { font-size:11px;}
                    th { text-align:left; padding-right:30px; font-size:11px;}
                    thead th { border-bottom: 1px solid #000;}
            </style>
            </head>
            <body>
                <div id="content">
                    <h1>Sitemap</h1>
                    <p>Sitemap contains <xsl:value-of select="count(s:urlset/s:url)"/> URLs</p>
                    <table id="sitemap" cellpadding="3" width="100%">
                        <thead>
                            <tr>
                                <th width="70%">URL</th>
                                <th width="30%">Last Modified</th>
                            </tr>
                            </thead>
                        <tbody>
                            <xsl:for-each select="s:urlset/s:url">
                                <tr>
                                    <xsl:variable name="link">
                                        <xsl:value-of select="s:loc"/>
                                    </xsl:variable>
                                    <td>
                                        <a href="{$link}">
                                            <xsl:value-of select="s:loc"/>
                                        </a>
                                    </td>
                                    <td>
                                        <xsl:value-of select="s:lastmod"/>
                                    </td>
                                </tr>
                            </xsl:for-each>
                        </tbody>
                    </table>
                </div>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>