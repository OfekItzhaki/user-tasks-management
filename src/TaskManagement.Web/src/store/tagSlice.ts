import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { Tag } from '../types';
import { tagApi } from '../services/api';

interface TagState {
  tags: Tag[];
  loading: boolean;
  error: string | null;
}

const initialState: TagState = {
  tags: [],
  loading: false,
  error: null,
};

export const fetchTags = createAsyncThunk('tags/fetchAll', async () => {
  return await tagApi.getAll();
});

const tagSlice = createSlice({
  name: 'tags',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchTags.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTags.fulfilled, (state, action) => {
        state.loading = false;
        state.tags = action.payload;
      })
      .addCase(fetchTags.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch tags';
      });
  },
});

export default tagSlice.reducer;
