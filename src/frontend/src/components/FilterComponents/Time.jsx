import React from 'react';
import { Button, Form, Icon, Input, Popup } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';
import Text from '../BaseComponents/Text';

const Facet = ({ name, text, value, onChange, sort, setSort }) => {
    const { t } = useTranslation();

    const getStringItem = i => {
        const parts = (value || '').split('-');
        return parts[i] || null;
    };

    const getDateItem = i => {
        let s = getStringItem(i);
        return s;
    };

    const callbackOnChange = (start, end) => {
        let value = start || end ? [start, end].join('-') : '';
        if (onChange !== undefined) onChange(new Event('change'), { name, value });
    };

    const toggleStart = (e, { value }) => {
        let start = value;
        if (start == getStringItem(0)) start = null;
        callbackOnChange(start, getStringItem(1));
    };

    const toggleEnd = (e, { value }) => {
        let end = value;
        if (end == getStringItem(1)) end = null;
        callbackOnChange(getStringItem(0), end);
    };

    const input = (
        <Input
            fluid
            name={name}
            autoComplete="off"
            autoFocus
            value={value || ''}
            placeholder={t(name)}
            label={{ basic: true, content: '' }}
            labelPosition="right"
            onChange={onChange}
        />
    );

    const content = (
        <Form className="filter-popup">
            {/* <div>{t(name)}</div>*/}
            <Form.Group>
                <Form.Field width={8}>
                    <Text type="time" value={getDateItem(0) || null} onChange={toggleStart} />
                </Form.Field>
                <span className="separator">-</span>
                <Form.Field width={8}>
                    <Text type="time" value={getDateItem(1) || null} onChange={toggleEnd} />
                </Form.Field>
            </Form.Group>
        </Form>
    );

    return (
        <div className="facet-input">
            {content}
            {/*<Popup
                trigger={input}
                content={content}
                on="click"
                hideOnScroll
                className="from-popup"
                position="bottom center"
            />
            <Button
                className={`sort-button sort-button-up ${
                    sort === 'asc' ? 'sort-button-active' : ''
                }`}
                name={name}
                value="asc"
                onClick={setSort}
            >
                <Icon name="caret up" />
            </Button>
            <Button
                className={`sort-button sort-button-down ${
                    sort === 'desc' ? 'sort-button-active' : ''
                }`}
                name={name}
                value="desc"
                onClick={setSort}
            >
                <Icon name="caret down" />
            </Button>*/}
        </div>
    );
};

export default Facet;
