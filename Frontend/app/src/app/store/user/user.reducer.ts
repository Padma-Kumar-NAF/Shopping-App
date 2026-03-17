import { createReducer, on } from '@ngrx/store';
import { initialState } from './user.state';
import { setUser, clearUser } from './user.actions';

export const userReducer = createReducer(
  initialState,

  on(setUser, (state, { user }) => ({
    ...state,
    user
  })),

  on(clearUser, (state) => ({
    ...state,
    user: null
  }))
);