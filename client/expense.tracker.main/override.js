const fs = require('fs');
const path = require('path');

const indexPath = path.join(__dirname, 'auto/autoexpensetrackerclient/index.ts');

function addOverrideKeyword() {
    fs.readFile(indexPath, 'utf8', (err, data) => {
        if (err) {
            console.error('Error reading index.ts:', err);
            return;
        }

        const messageRegex = /(\s*message:\s*string\s*;)/;
        const overrideMessageRegex = /(\s*override\s+message:\s*string\s*;)/;

        let updatedData = data;

        // Fix the message override issue
        if (!overrideMessageRegex.test(data) && messageRegex.test(data)) {
            updatedData = updatedData.replace(messageRegex, '\n\toverride message: string;');
            console.log('Successfully added override keyword to `message`');
        }

        // Fix array type casting issues - generic pattern that handles any array type
        const arrayTypeCastingPattern = /return _observableOf<(\w+\[\])>\(null as any\);/g;
        const arrayMatches = [...updatedData.matchAll(arrayTypeCastingPattern)];
        
        let hasArrayFixes = false;
        if (arrayMatches.length > 0) {
            // Replace all array type casting issues generically
            updatedData = updatedData.replace(arrayTypeCastingPattern, (match, arrayType) => {
                hasArrayFixes = true;
                return `return _observableOf<${arrayType}>(null as any as ${arrayType});`;
            });
        }

        if (hasArrayFixes) {
            console.log('Successfully fixed array type casting issues');
        }

        // Write the file if any changes were made
        if (updatedData !== data) {
            fs.writeFile(indexPath, updatedData, 'utf8', (err) => {
                if (err) {
                    console.error('Error writing to index.ts:', err);
                } else {
                    console.log('Successfully updated index.ts');
                }
            });
        } else {
            console.log('No changes needed');
        }
    });
}

addOverrideKeyword();