import React from 'react';
import { Input } from 'semantic-ui-react';
import { withTranslation } from 'react-i18next';

import './style.scss';

class Search extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            value: '',
        };
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        if (this.props.value !== this.state.value && this.props.value !== prevProps.value) {
            this.setState({ value: this.props.value });
        }
    }

    componentWillMount() {
        this.timer = null;
    }

    triggerChange = () => {
        const { value } = this.state;

        this.props.onChange(null, { value });
    };

    handleChange = (e, { value }) => {
        clearTimeout(this.timer);

        this.setState({ value });

        if (!value || this.props.isAuto) {
            this.timer = setTimeout(this.triggerChange, 300);
        }
    };

    handleKeyPress = e => {
        if (e.keyCode === 13) {
            clearTimeout(this.timer);
            this.triggerChange();
        }
    };

    /*change = (e, { value }) => {
        const { isAuto } = this.props;
        this.setState({ value });
        if (isAuto || !value) this.props.onChange(e, { value });
    };*/

    render() {
        const { t, placeholder, fluid, size, className, autoFocus, innerRef } = this.props;

        return (
            <Input
                icon="search"
                className={className}
                fluid={fluid}
                size={size}
                ref={innerRef}
                onKeyDown={this.handleKeyPress}
                onChange={this.handleChange}
                placeholder={placeholder === undefined ? t('search_all_fields') : placeholder}
                value={this.state.value}
            />
        );
    }
}

const TranslateMyComponent = withTranslation()(Search);

export default React.forwardRef((props, ref) => <TranslateMyComponent
    innerRef={ref} {...props}
/>);
