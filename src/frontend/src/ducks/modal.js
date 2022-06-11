import { createSelector } from 'reselect';

//*  TYPES  *//

const SHOW_MODAL = 'SHOW_MODAL';
const HIDE_MODAL = 'HIDE_MODAL';

//*  INITIAL STATE  *//

const initial = {
    open: false,
    content: {}
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case SHOW_MODAL:
            return {
                ...state,
                open: true,
                content: payload
            };
        case HIDE_MODAL:
            return {
                ...state,
                ...initial
            };
        default:
            return state;
    }
}

//*  ACTION CREATORS  *//
export const showModal = payload => {
    return {
        type: SHOW_MODAL,
        payload
    }
};

export const hideModal = () => {
    return {
        type: HIDE_MODAL,
    }
};

//*  SELECTORS *//

const stateSelector = state => state.modal;

export const isOpenSelector = createSelector(stateSelector, state => state.open);
export const contentSelector = createSelector(stateSelector, state => state.content);
