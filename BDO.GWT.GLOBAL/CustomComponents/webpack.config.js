const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');

module.exports = {
    entry: { entry: './entry.tsx' },
    output: {
        filename: 'entry.js',
        path: path.resolve(__dirname, '../wwwroot/Admin/adminresources/bdo.custom.components'),
        library: { type: 'system' }
    },
    plugins: [
        new MiniCssExtractPlugin({ filename: 'entry.css' })
    ],
    optimization: {
        minimizer: [
            `...`,
            new CssMinimizerPlugin({
                minimizerOptions: {
                    preset: [
                        'default',
                        {
                            discardComments: { removeAll: true },
                            normalizeWhitespace: true,
                            minifyFontValues: true,
                            minifyGradients: true,
                        }
                    ]
                }
            }),
        ],
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js'],
        alias: {
            '~': path.resolve(__dirname, 'src'),
            '@components': path.resolve(__dirname, 'src/components'),
            '@model': path.resolve(__dirname, 'src/model'),
            '@styles': path.resolve(__dirname, 'src/styles')
        }
    },
    module: {
        rules: [
            { test: /\.tsx?$/, use: 'ts-loader', exclude: /node_modules/ },
            {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    { loader: 'css-loader', options: { importLoaders: 1 } },
                    'postcss-loader'
                ]
            }
        ]
    },
    externals: {
        'react': 'react',
        'react-dom': 'react-dom',
        '@kentico/xperience-admin-base': '@kentico/xperience-admin-base',
        '@kentico/xperience-admin-components': '@kentico/xperience-admin-components'
    }
};
