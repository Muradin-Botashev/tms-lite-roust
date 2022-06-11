import React, {Suspense} from 'react';
import ReactDOM from 'react-dom';
import {Provider} from 'react-redux';
import './utils/i18n';
import {registerLocale} from 'react-datepicker';
import ru from 'date-fns/locale/ru';
import enGB from 'date-fns/locale/en-GB';

import store from './store/configureStore';

import App from './containers/App';
import {Dimmer, Loader} from 'semantic-ui-react';

import 'semantic-ui-css/semantic.min.css';
import 'react-datepicker/dist/react-datepicker.min.css';
import 'react-toastify/dist/ReactToastify.min.css';
import 'react-resizable/css/styles.css';
import './style/main.scss';

registerLocale('ru', ru);
registerLocale('en', enGB);

String.prototype.replaceAll = function (search, replacement) {
    const target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

ReactDOM.render(
    <Provider store={store}>
        <Suspense
            fallback={
                <Dimmer active inverted className="table-loader">
                    <Loader size="huge">Loading</Loader>
                </Dimmer>
            }
        >
            <App/>
        </Suspense>
    </Provider>,
    document.getElementById('root'),
);
