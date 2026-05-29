const path = require('path');

module.exports = {
    entry: { 'entry': './entry.tsx' },
    output: {
        filename: 'entry.js',
        path: path.resolve(__dirname, '../wwwroot/Admin/adminresources/bdo.custom.dropdown'),
        library: { type: 'system' }
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js'],
        alias: { '~': path.resolve(__dirname, '.') }
    },
    module: {
        rules: [{ test: /\.tsx?$/, use: 'ts-loader', exclude: /node_modules/ }]
    },
    externals: {
        'react': 'react',
        'react-dom': 'react-dom',
        '@kentico/xperience-admin-base': '@kentico/xperience-admin-base',
        '@kentico/xperience-admin-components': '@kentico/xperience-admin-components'
    }
};