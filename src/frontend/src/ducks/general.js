import { select, take, spawn, put, delay, cancel, cancelled, call, fork } from 'redux-saga/effects';
import { createSelector } from 'reselect';
import { clearDictionaryInfo } from './dictionaryView';
import { autoUpdateStop } from './gridList';


const routeSelector = state => state.router;

export const currentLocationSelector = createSelector(routeSelector, state => state.location.pathname);


function* changeLocation() {
    while (true) {
        const currentLocation = yield select(currentLocationSelector);
        const { payload } = yield take('@@router/LOCATION_CHANGE');
        const { location } = payload;
        const { pathname } = location;

        if (pathname.includes('dictionary') && pathname !== currentLocation) {
            yield put(clearDictionaryInfo());
        }

        if (pathname.includes('grid') && pathname !== currentLocation) {
            yield put(
                autoUpdateStop({
                    isClear: true,
                }),
            );
        }
    }
}

export function* saga() {
    yield spawn(changeLocation);
}
