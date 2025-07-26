const fs = require('fs');
const path = require('path');

// Path to the generated API client file
const apiClientPath = path.join(__dirname, 'genClients', 'api-client.ts');

// Check if the file exists
if (fs.existsSync(apiClientPath)) {
  let content = fs.readFileSync(apiClientPath, 'utf8');
  
  // Add missing imports at the top
  const importsToAdd = `import { Injectable, InjectionToken, Inject, Optional } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';

`;

  // Check if imports are already present
  if (!content.includes('@angular/core')) {
    content = importsToAdd + content;
  }

  // Fix any compilation issues by ensuring proper Observable usage
  content = content.replace(/Observable\.throw/g, '_observableThrow');
  content = content.replace(/Observable\.of/g, '_observableOf');
  
  // Write the modified content back
  fs.writeFileSync(apiClientPath, content, 'utf8');
  
  console.log('✅ Business layer API client generated and enhanced successfully!');
} else {
  console.log('❌ API client file not found. Make sure NSwag generation completed successfully.');
}
