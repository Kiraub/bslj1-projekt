#include <stdio.h>
#include <stdlib.h>

#define ROW_MAX 3
#define COLUMN_MAX 4

// matrix data
// input parsing isnt built in, just edit this array and above matrix dimensions as needed
float data[] = {2,1,-1,8, -3,-1,2,-11, -2,1,2,-3};

void printMatrix(float** matrix, int rows, int columns) {
    printf("\n");
    for (int row = 0; row < rows; row += 1) {
        printf("|");
        for (int column= 0; column < columns; column += 1) {
            printf("%6.2f ", matrix[row][column]);
        }
        printf("|\n");
    }
    printf("\n");
}

int argmax(int start_row, int max_row, int pivot_column, float** matrix) {
    float max = 0;
    int ret_row = start_row;
    for (int row = start_row; row < max_row; row += 1) {
        float row_val = abs(matrix[row][pivot_column]);
        if ( row_val > max ) {
            max = row_val;
            ret_row = row;
        }
    }
    return ret_row;
}

void row_swap(float** matrix, int row1, int row2) {
    if(row1 != row2) {
        printf("    SWAP: %d <-> %d\n", row1+1, row2+1);
        float* temp_pointer = matrix[row1];
        matrix[row1] = matrix[row2];
        matrix[row2] = temp_pointer;
    } else {
        printf("    NOSWAP\n");
    }
    return;
}

void row_reduce(float** matrix, int target_row, int reduce_row, int column_max, float mul) {
    printf("    REDUCE R%d by R%d * %3.2f\n", target_row+1, reduce_row+1, mul);
    for (int cc = 0; cc < column_max; cc += 1) {
        matrix[target_row][cc] -= matrix[reduce_row][cc]*mul;
    }
}

void gauss_elim(float** matrix, int rows, int columns) {
    int h = 0;
    int k = 0;
    int m = rows;
    int n = columns;
    printf("Gauss-Elim\n");
    while (h < m && k < n) {
        /* Find the k-th pivot */
        int i_max = argmax( h, m, k, matrix);
        if (matrix[i_max][k] == 0) {
            /* No pivot in this column, pass to next column */
            k += 1;
        } else {
            row_swap(matrix, h, i_max);
            /* Do for all rows below pivot */
            for (int i = h+1; i < m; i += 1) {
                float factor = ((float)matrix[i][k]) / ((float)matrix[h][k]);
                /* Fill the lower part of pivot column with zeroes */
                matrix[i][k] = 0;
                /* Do for all remaining elements in current row */
                //row_reduce(matrix, i, h, COLUMN_MAX, factor);
                printf("    SUB: R%d - R%d * %.2f\n", i+1, h+1, factor);
                for (int j = k+1; j < n; j += 1) {
                    matrix[i][j] = matrix[i][j] - matrix[h][j] * factor;
                }
            }
            //printMatrix(matrix, ROW_MAX, COLUMN_MAX);
            /* Increase pivot row and column */
            h += 1;
            k += 1;
        }
    }
    printf("Normalization\n");
    for ( int dc = 0; dc < m && dc < n; dc += 1) {
        float mul_factor = 1 / matrix[dc][dc];
        for  ( int cc = dc; cc < n; cc += 1) {
            matrix[dc][cc] *= mul_factor;
        }
    }
    printMatrix(matrix, ROW_MAX, COLUMN_MAX);
    printf("Jordan-Elim\n");
    for ( int rc = 0; rc < m-1; rc += 1) {
        for ( int rr = rc+1; rr < m; rr += 1) {
            row_reduce(matrix, rc, rr, COLUMN_MAX, matrix[rc][rr]);
        }
    }
    printMatrix(matrix, ROW_MAX, COLUMN_MAX);
    printf("Solution\n");
    for ( int ac = 0; ac < m; ac += 1) {
        printf("    %c = %.4f\n", 'a'+ac, matrix[ac][n-1]);
    }
}


int main(int argc, char* argv[]) {
    printf("Gauss-Jordan-Elimination Algorithm:\n");

    int data_counter = 0;

    float** matrix;

    matrix = malloc(sizeof(float*) * ROW_MAX);

    for (int row = 0; row < ROW_MAX; row += 1) {
        matrix[row] = malloc(sizeof(float) * COLUMN_MAX);
        for (int column= 0; column < COLUMN_MAX; column += 1) {
            matrix[row][column] = data[data_counter];
            data_counter += 1;
        }
    }

    printMatrix(matrix, ROW_MAX, COLUMN_MAX);
    //row_swap(matrix, 0, 1);
    gauss_elim(matrix, ROW_MAX, COLUMN_MAX);
    for (int row = 0; row < ROW_MAX; row += 1) {
        free(matrix[row]);
    }
    free(matrix);

    return(0);
}
