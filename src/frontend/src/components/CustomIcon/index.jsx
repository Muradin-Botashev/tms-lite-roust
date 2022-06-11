import React, { Component } from 'react';
import '../../icons/icon/style.css';
import PropTypes from 'prop-types';

class Icon extends Component {
    render() {
        const { color, size, name, className, onClick } = this.props,
            style = { color };

        return (
            <i
                className={`icon icon-${name} icon-${size} ${className}`}
                style={style}
                onClick={onClick}
            />
        );
    }
}

Icon.propTypes = {
    color: PropTypes.string,
    name: PropTypes.string.isRequired,
    size: PropTypes.oneOf([9, 12, 14, 16, 24, 32, 40, 48]),
    className: PropTypes.string,
    onClick: PropTypes.func,
};

Icon.defaultProps = {
    color: 'rgba(0,0,0,.6)',
    onClick: () => {},
};

export default Icon;
