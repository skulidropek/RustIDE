import React from 'react';
import { CompilationResult } from '../api-client'; 

interface ConsoleOutputProps {
  errors: CompilationResult[]; 
}

const ConsoleOutput: React.FC<ConsoleOutputProps> = ({ errors }) => {
  return (
    <div style={{ backgroundColor: '#1e1e1e', color: '#d4d4d4', padding: '10px', height: '200px', overflowY: 'scroll' }}>
      {errors.length === 0 ? (
        <div>No errors.</div>
      ) : (
        errors.map((error, index) => (
          <div key={index}>
            {error.errors && error.errors.length > 0 && (
              <>
                <div style={{ color: 'red' }}>Error {index + 1}:</div>
                {error.errors.map((err, errIndex) => (
                  <div key={errIndex}>
                    Line {err.startLine}, Column {err.startColumn}: {err.message}
                  </div>
                ))}
              </>
            )}
          </div>
        ))
      )}
    </div>
  );
};

export default ConsoleOutput;
